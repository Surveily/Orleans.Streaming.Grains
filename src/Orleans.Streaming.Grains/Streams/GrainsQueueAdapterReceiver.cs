// <copyright file="GrainsQueueAdapterReceiver.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Providers.Streams.Common;
using Orleans.Serialization;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Streams
{
    public class GrainsQueueAdapterReceiver : IQueueAdapterReceiver
    {
        private readonly ILogger _logger;
        private readonly QueueId _queueId;
        private readonly List<Task> _awaitingTasks;
        private readonly ITransactionService _service;
        private readonly IStreamQueueMapper _streamQueueMapper;
        private readonly Serializer<GrainsBatchContainer> _serializer;
        private readonly IQueueAdapterReceiverMonitor _receiverMonitor;

        private long _lastReadMessage;

        public GrainsQueueAdapterReceiver(ILogger logger,
                                          QueueId queueId,
                                          ITransactionService service,
                                          IStreamQueueMapper streamQueueMapper,
                                          Serializer<GrainsBatchContainer> serializer,
                                          IQueueAdapterReceiverMonitor receiverMonitor)
        {
            _logger = logger;
            _queueId = queueId;
            _service = service;
            _serializer = serializer;
            _receiverMonitor = receiverMonitor;
            _streamQueueMapper = streamQueueMapper;

            _awaitingTasks = new List<Task>();
        }

        public Task Initialize(TimeSpan timeout)
        {
            _receiverMonitor?.TrackInitialization(true, TimeSpan.MinValue, null);

            return Task.CompletedTask;
        }

        public async Task<IList<IBatchContainer>> GetQueueMessagesAsync(int maxCount)
        {
            var watch = Stopwatch.StartNew();

            List<IBatchContainer> batches;
            Task<List<(Guid Id, Immutable<GrainsMessage> Item)?>> task = null;

            try
            {
                task = Task.Run(async () =>
                {
                    var messages = new List<(Guid Id, Immutable<GrainsMessage> Item)?>();

                    do
                    {
                        var message = await _service.PopAsync<GrainsMessage>(_queueId.ToString());

                        if (message != null && message.HasValue && message.Value.Item.Value != null)
                        {
                            messages.Add(message);
                        }
                        else
                        {
                            break;
                        }
                    }
                    while (messages.Count < maxCount);

                    return messages;
                });

                _awaitingTasks.Add(task);

                var eventData = await task;

                batches = eventData.Select(data => GrainsBatchContainer.FromMessage(_serializer, data.Value.Id, data.Value.Item.Value, _lastReadMessage++)).ToList<IBatchContainer>();

                watch.Stop();

                _receiverMonitor?.TrackRead(true, watch.Elapsed, null);

                if (eventData.Count > 0)
                {
                    var oldestMessage = eventData[0].Value.Item.Value.EnqueueTimeUtc;
                    var newestMessage = eventData[eventData.Count - 1].Value.Item.Value.EnqueueTimeUtc;

                    _receiverMonitor?.TrackMessagesReceived(batches.Count, oldestMessage, newestMessage);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Exception thrown in {nameof(GrainsQueueAdapterReceiver)}.{nameof(GetQueueMessagesAsync)}.");

                watch.Stop();

                _receiverMonitor?.TrackRead(true, watch.Elapsed, ex);

                throw;
            }
            finally
            {
                _awaitingTasks.Remove(task);
            }

            return batches;
        }

        public async Task MessagesDeliveredAsync(IList<IBatchContainer> messages)
        {
            foreach (var message in messages.OfType<GrainsBatchContainer>())
            {
                var queue = _streamQueueMapper.GetQueueForStream(message.StreamId);

                await _service.CompleteAsync<GrainsMessage>(message.Id, true, queue.ToString());
            }
        }

        public async Task Shutdown(TimeSpan timeout)
        {
            var watch = Stopwatch.StartNew();

            try
            {
                if (_awaitingTasks.Count != 0)
                {
                    await Task.WhenAll(_awaitingTasks);
                }

                watch.Stop();
                _receiverMonitor?.TrackShutdown(true, watch.Elapsed, null);
            }
            catch (Exception ex)
            {
                watch.Stop();
                _receiverMonitor?.TrackShutdown(false, watch.Elapsed, ex);
            }
        }
    }
}