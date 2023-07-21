// <copyright file="GrainsQueueAdapterFactory.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Configuration.Overrides;
using Orleans.Providers.Streams.Common;
using Orleans.Serialization;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Streams
{
    public class GrainsQueueAdapterFactory : IQueueAdapterFactory
    {
        private readonly string _name;

        private readonly Serializer _serializer;
        private readonly ITransactionService _service;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IQueueAdapterCache _adapterCache;

        private readonly HashRingBasedStreamQueueMapper _streamQueueMapper;

        public GrainsQueueAdapterFactory(string name,
                                         Serializer serializer,
                                         ITransactionService service,
                                         ILoggerFactory loggerFactory,
                                         SimpleQueueCacheOptions cacheOptions,
                                         HashRingStreamQueueMapperOptions queueMapperOptions)
        {
            _name = name;
            _service = service;
            _serializer = serializer;
            _loggerFactory = loggerFactory;
            _streamQueueMapper = new HashRingBasedStreamQueueMapper(queueMapperOptions, _name);
            _adapterCache = new SimpleQueueAdapterCache(cacheOptions, _name, _loggerFactory);
        }

        public static GrainsQueueAdapterFactory Create(IServiceProvider services, string name)
        {
            var clusterOptions = services.GetProviderClusterOptions(name);
            var cacheOptions = services.GetOptionsByName<SimpleQueueCacheOptions>(name);
            var queueMapperOptions = services.GetOptionsByName<HashRingStreamQueueMapperOptions>(name);

            return ActivatorUtilities.CreateInstance<GrainsQueueAdapterFactory>(services, name, queueMapperOptions, cacheOptions, services, clusterOptions);
        }

        public Task<IQueueAdapter> CreateAdapter()
        {
            var adapter = new GrainsQueueAdapter(_serializer, _service, _streamQueueMapper, _name);

            return Task.FromResult<IQueueAdapter>(adapter);
        }

        public Task<IStreamFailureHandler> GetDeliveryFailureHandler(QueueId queueId)
        {
            return Task.FromResult<IStreamFailureHandler>(new NoOpStreamDeliveryFailureHandler());
        }

        public IQueueAdapterCache GetQueueAdapterCache()
        {
            return _adapterCache;
        }

        public IStreamQueueMapper GetStreamQueueMapper()
        {
            return _streamQueueMapper;
        }
    }
}