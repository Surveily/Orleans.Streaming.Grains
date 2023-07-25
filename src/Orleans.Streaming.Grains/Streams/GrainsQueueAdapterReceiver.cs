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
        }

        public async Task<IList<IBatchContainer>> GetQueueMessagesAsync(int maxCount)
        {
            var resultBag = new ConcurrentBag<IBatchContainer>();

            await Parallel.ForEachAsync(_streamQueueMapper.GetAllQueues(), async (queue, token) =>
            {
                const int MaxNumberOfMessagesToPeek = 256;

                var fetched = 0;
                var result = new List<IBatchContainer>();
                var count = maxCount < 0 || maxCount == QueueAdapterConstants.UNLIMITED_GET_QUEUE_MSG ?
                       MaxNumberOfMessagesToPeek : Math.Min(maxCount, MaxNumberOfMessagesToPeek);

                (Guid Id, Immutable<GrainsMessage> Item)? message;

                do
                {
                    message = await _service.PopAsync<GrainsMessage>(queue.ToString());

                    if (message != null)
                    {
                        resultBag.Add(GrainsBatchContainer.FromMessage(_serializationManager, message.Value.Id, message.Value.Item.Value, _lastReadMessage++));
                    }
                }
                while (message != null && ++fetched < count);
            });

            return resultBag.ToList();
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