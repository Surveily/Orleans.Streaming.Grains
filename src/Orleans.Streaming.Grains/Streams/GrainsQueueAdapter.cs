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
using Orleans.Providers.Streams.Common;
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
        private readonly IStreamQueueMapper _streamQueueMapper;
        private readonly Serializer<GrainsBatchContainer> _serializer;

        public GrainsQueueAdapter(string providerName,
                                  Serializer serializer,
                                  GrainsOptions options,
                                  ITransactionService service,
                                  IStreamQueueMapper streamQueueMapper)
        {
            _options = options;
            _service = service;
            _providerName = providerName;
            _streamQueueMapper = streamQueueMapper;
            _serializer = serializer.GetSerializer<GrainsBatchContainer>();
        }

        public bool IsRewindable => false;

        public string Name => _providerName;

        public StreamProviderDirection Direction => StreamProviderDirection.ReadWrite;

        public IQueueAdapterReceiver CreateReceiver(QueueId queueId) => new GrainsQueueAdapterReceiver(queueId, _service, _streamQueueMapper, _serializer);

        public async Task QueueMessageBatchAsync<T>(StreamId streamId, IEnumerable<T> events, StreamSequenceToken token, Dictionary<string, object> requestContext)
        {
            var queue = _streamQueueMapper.GetQueueForStream(streamId);
            var message = GrainsBatchContainer.ToMessage(_serializer, streamId, events, requestContext);

            await _service.PostAsync(new Immutable<GrainsMessage>(message), !_options.FireAndForgetDelivery, queue.ToString());
        }
    }
}