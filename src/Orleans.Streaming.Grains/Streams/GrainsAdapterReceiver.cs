// <copyright file="GrainsAdapterReceiver.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Providers;
using Orleans.Providers.Streams.Common;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Streams
{
    internal class GrainsAdapterReceiver<TSerializer> : IQueueAdapterReceiver
        where TSerializer : class, IMemoryMessageBodySerializer
    {
        private readonly IMemoryStreamQueueGrain _queueGrain;
        private readonly List<Task> _awaitingTasks;
        private readonly ILogger _logger;
        private readonly TSerializer _serializer;
        private readonly IQueueAdapterReceiverMonitor _receiverMonitor;

        public GrainsAdapterReceiver(IMemoryStreamQueueGrain queueGrain, ILogger logger, TSerializer serializer, IQueueAdapterReceiverMonitor receiverMonitor)
        {
            _queueGrain = queueGrain;
            _logger = logger;
            _serializer = serializer;
            _awaitingTasks = new List<Task>();
            _receiverMonitor = receiverMonitor;
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
            Task<List<MemoryMessageData>> task = null;
            try
            {
                task = _queueGrain.Dequeue(maxCount);
                _awaitingTasks.Add(task);
                var eventData = await task;
                batches = eventData.Select(data => new GrainsBatchContainer<TSerializer>(data, _serializer)).ToList<IBatchContainer>();
                watch.Stop();
                _receiverMonitor?.TrackRead(true, watch.Elapsed, null);
                if (eventData.Count > 0)
                {
                    var oldestMessage = eventData[0];
                    var newestMessage = eventData[eventData.Count - 1];
                    _receiverMonitor?.TrackMessagesReceived(eventData.Count, oldestMessage.EnqueueTimeUtc, newestMessage.EnqueueTimeUtc);
                }
            }
            catch (Exception exc)
            {
                _logger.LogError((int)ProviderErrorCode.MemoryStreamProviderBase_GetQueueMessagesAsync, exc, "Exception thrown in MemoryAdapterFactory.GetQueueMessagesAsync.");
                watch.Stop();
                _receiverMonitor?.TrackRead(true, watch.Elapsed, exc);
                throw;
            }
            finally
            {
                _awaitingTasks.Remove(task);
            }

            return batches;
        }

        public Task MessagesDeliveredAsync(IList<IBatchContainer> messages)
        {
            return Task.CompletedTask;
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
