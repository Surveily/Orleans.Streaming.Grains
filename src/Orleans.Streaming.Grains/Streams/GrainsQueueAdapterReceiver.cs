// <copyright file="GrainsQueueAdapterReceiver.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
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
        private readonly string _stream;

        private readonly Serializer<GrainsBatchContainer> _serializationManager;
        private readonly ITransactionService _service;
        private TimeSpan _timeout;

        private long _lastReadMessage;

        public GrainsQueueAdapterReceiver(Serializer<GrainsBatchContainer> serializationManager,
                                          ITransactionService service,
                                          string stream)
        {
            if (stream == null)
            {
                throw new ArgumentException(nameof(stream));
            }

            _stream = stream;
            _service = service;
            _timeout = TimeSpan.FromSeconds(1);
            _serializationManager = serializationManager;
        }

        public async Task<IList<IBatchContainer>> GetQueueMessagesAsync(int maxCount)
        {
            const int MaxNumberOfMessagesToPeek = 256;

            var fetched = 0;
            var result = new List<IBatchContainer>();
            var count = maxCount < 0 || maxCount == QueueAdapterConstants.UNLIMITED_GET_QUEUE_MSG ?
                   MaxNumberOfMessagesToPeek : Math.Min(maxCount, MaxNumberOfMessagesToPeek);

            (Guid Id, Immutable<GrainsMessage> Item)? message;

            do
            {
                message = await _service.PopAsync<GrainsMessage>();

                if (message != null)
                {
                    result.Add(GrainsBatchContainer.FromMessage(_serializationManager, message.Value.Id, message.Value.Item.Value, _lastReadMessage++));
                }
            }
            while (message != null && ++fetched < count);

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
                await _service.CompleteAsync<GrainsMessage>(message.Id, true);
            }
        }

        public Task Shutdown(TimeSpan timeout)
        {
            return Task.CompletedTask;
        }
    }
}