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
using Orleans.Providers;
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
        private readonly ILoggerFactory _loggerFactory;
        private readonly GrainsOptions _options;
        private readonly ITransactionService _service;
        private readonly IStreamQueueMapper _streamQueueMapper;
        private readonly IMemoryMessageBodySerializer _serializer;

        public GrainsQueueAdapter(string providerName,
                                  IMemoryMessageBodySerializer serializer,
                                  GrainsOptions options,
                                  ITransactionService service,
                                  ILoggerFactory loggerFactory,
                                  IStreamQueueMapper streamQueueMapper)
        {
            _options = options;
            _service = service;
            _serializer = serializer;
            _providerName = providerName;
            _loggerFactory = loggerFactory;
            _streamQueueMapper = streamQueueMapper;
        }

        public bool IsRewindable => true;

        public string Name => _providerName;

        public StreamProviderDirection Direction => StreamProviderDirection.ReadWrite;

        public IQueueAdapterReceiver CreateReceiver(QueueId queueId)
        {
            var dimensions = new ReceiverMonitorDimensions(queueId.ToString());
            var logger = _loggerFactory.CreateLogger($"{typeof(GrainsQueueAdapter).FullName}.{_providerName}.{queueId}");
            var monitor = new DefaultQueueAdapterReceiverMonitor(dimensions);

            return new GrainsQueueAdapterReceiver(logger, queueId, _service, _streamQueueMapper, _serializer, monitor);
        }

        public async Task QueueMessageBatchAsync<T>(StreamId streamId, IEnumerable<T> events, StreamSequenceToken token, Dictionary<string, object> requestContext)
        {
            var queueId = _streamQueueMapper.GetQueueForStream(streamId);
            var bodyBytes = _serializer.Serialize(new MemoryMessageBody(events.Cast<object>(), requestContext));
            var message = new MemoryMessageData
            {
                StreamId = streamId,
                Payload = bodyBytes,
                EnqueueTimeUtc = DateTime.UtcNow,
            };

            await _service.PostAsync(new Immutable<MemoryMessageData>(message), !_options.FireAndForgetDelivery, queueId.ToString());
        }
    }
}