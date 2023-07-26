// <copyright file="GrainsQueueAdapter.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly GrainsOptions _options;
        private readonly ITransactionService _service;
        private readonly ILoggerFactory _loggerFactory;
        private readonly Serializer<GrainsBatchContainer> _serializer;
        private readonly IConsistentRingStreamQueueMapper _streamQueueMapper;

        public GrainsQueueAdapter(Serializer serializer,
                                  GrainsOptions options,
                                  ITransactionService service,
                                  IConsistentRingStreamQueueMapper streamQueueMapper,
                                  ILoggerFactory loggerFactory,
                                  string providerName)
        {
            _options = options;
            _service = service;
            _providerName = providerName;
            _loggerFactory = loggerFactory;
            _streamQueueMapper = streamQueueMapper;
            _serializer = serializer.GetSerializer<GrainsBatchContainer>();
        }

        public bool IsRewindable => false;

        public string Name => _providerName;

        public StreamProviderDirection Direction => StreamProviderDirection.ReadWrite;

        public IQueueAdapterReceiver CreateReceiver(QueueId queueId) => new GrainsQueueAdapterReceiver(_service, _serializer, _streamQueueMapper);

        public async Task QueueMessageBatchAsync<T>(StreamId streamId, IEnumerable<T> events, StreamSequenceToken token, Dictionary<string, object> requestContext)
        {
            var queue = _streamQueueMapper.GetQueueForStream(streamId);
            var message = GrainsBatchContainer.ToMessage(_serializer, streamId, events, requestContext);

            await _service.PostAsync(new Immutable<GrainsMessage>(message), !_options.FireAndForgetDelivery, queue.ToString());
        }
    }
}