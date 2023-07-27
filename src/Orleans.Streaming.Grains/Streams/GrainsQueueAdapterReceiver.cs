// <copyright file="GrainsQueueAdapterReceiver.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Orleans.Serialization;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Streams
{
    public class GrainsQueueAdapterReceiver : IQueueAdapterReceiver
    {
        private readonly IList<QueueId> _queues;
        private readonly ITransactionService _service;
        private readonly IConsistentRingStreamQueueMapper _streamQueueMapper;
        private readonly Serializer<GrainsBatchContainer> _serializationManager;

        private long _lastReadMessage;

        public GrainsQueueAdapterReceiver(ITransactionService service,
                                          Serializer<GrainsBatchContainer> serializationManager,
                                          IConsistentRingStreamQueueMapper streamQueueMapper)
        {
            _service = service;
            _streamQueueMapper = streamQueueMapper;
            _serializationManager = serializationManager;
            _queues = _streamQueueMapper.GetAllQueues()
                                        .ToList();
        }

        public async Task<IList<IBatchContainer>> GetQueueMessagesAsync(int maxCount)
        {
            var result = new List<IBatchContainer>();
            var messages = await Task.WhenAll(_queues.Take(maxCount)
                                                     .Select(x => Task.Run(async () => await _service.PopAsync<GrainsMessage>(x.ToString()))));

            result.AddRange(messages.Where(x => x != null)
                                    .Select(x => GrainsBatchContainer.FromMessage(_serializationManager, x.Value.Id, x.Value.Item.Value, _lastReadMessage++)));

            return result;
        }

        public Task Initialize(TimeSpan timeout)
        {
            return Task.CompletedTask;
        }

        public async Task MessagesDeliveredAsync(IList<IBatchContainer> messages)
        {
            foreach (var message in messages.OfType<GrainsBatchContainer>())
            {
                var queue = _streamQueueMapper.GetQueueForStream(message.StreamId);

                await _service.CompleteAsync<GrainsMessage>(message.Id, true, queue.ToString());
            }
        }

        public Task Shutdown(TimeSpan timeout)
        {
            return Task.CompletedTask;
        }
    }
}