// <copyright file="GrainsQueueAdapter.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Streams
{
    public class GrainsQueueAdapter : IQueueAdapter
    {
        private readonly string _providerName;

        private readonly ILoggerFactory _loggerFactory;

        private readonly Serializer<GrainsBatchContainer> _serializer;
        private readonly ITransactionService _service;
        private readonly IConsistentRingStreamQueueMapper _streamQueueMapper;

        public GrainsQueueAdapter(Serializer serializer,
                                  ITransactionService service,
                                  IConsistentRingStreamQueueMapper streamQueueMapper,
                                  ILoggerFactory loggerFactory,
                                  string providerName)
        {
            _service = service;
            _loggerFactory = loggerFactory;
            _streamQueueMapper = streamQueueMapper;
            _serializer = serializer.GetSerializer<GrainsBatchContainer>();
            _providerName = providerName;
        }

        public bool IsRewindable => false;

        public string Name => _providerName;

        public StreamProviderDirection Direction => StreamProviderDirection.ReadWrite;

        public IQueueAdapterReceiver CreateReceiver(QueueId queueId) => new GrainsQueueAdapterReceiver(_serializer, _service, queueId.ToString());

        public async Task QueueMessageBatchAsync<T>(StreamId streamId, IEnumerable<T> events, StreamSequenceToken token, Dictionary<string, object> requestContext)
        {
            var queueId = _streamQueueMapper.GetQueueForStream(streamId);
            var message = GrainsBatchContainer.ToMessage(_serializer, streamId, events, requestContext);

            await _service.PostAsync(new Immutable<GrainsMessage>(message));
        }
    }
}