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
        private readonly QueueId _queueId;
        private readonly ITransactionService _service;
        private readonly IStreamQueueMapper _streamQueueMapper;
        private readonly Serializer<GrainsBatchContainer> _serializer;

        private long _lastReadMessage;

        public GrainsQueueAdapterReceiver(QueueId queueId,
                                          ITransactionService service,
                                          IStreamQueueMapper streamQueueMapper,
                                          Serializer<GrainsBatchContainer> serializer)
        {
            _queueId = queueId;
            _service = service;
            _serializer = serializer;
            _streamQueueMapper = streamQueueMapper;
        }

        public async Task<IList<IBatchContainer>> GetQueueMessagesAsync(int maxCount)
        {
            var result = new List<IBatchContainer>();

            do
            {
                var message = await _service.PopAsync<GrainsMessage>(_queueId.ToString());

                if (message != null && message.HasValue && message.Value.Item.Value != null)
                {
                    result.Add(GrainsBatchContainer.FromMessage(_serializer, message.Value.Id, message.Value.Item.Value, _lastReadMessage++));
                }
                else
                {
                    break;
                }
            }
            while (result.Count < maxCount);

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