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
        private readonly IServiceProvider _serviceProvider;
        private readonly SimpleQueueCacheOptions _cacheOptions;
        private readonly IOptions<GrainsOptions> _grainsOptions;
        private readonly IOptions<ClusterOptions> _clusterOptions;
        private readonly HashRingBasedStreamQueueMapper _streamQueueMapper;

        public GrainsQueueAdapterFactory(string name,
                                         Serializer serializer,
                                         ITransactionService service,
                                         ILoggerFactory loggerFactory,
                                         IServiceProvider serviceProvider,
                                         IOptions<GrainsOptions> grainsOptions,
                                         SimpleQueueCacheOptions cacheOptions,
                                         IOptions<ClusterOptions> clusterOptions,
                                         IOptions<HashRingStreamQueueMapperOptions> queueMapperOptions)
        {
            _name = name;
            _service = service;
            _serializer = serializer;
            _cacheOptions = cacheOptions;
            _grainsOptions = grainsOptions;
            _loggerFactory = loggerFactory;
            _clusterOptions = clusterOptions;
            _serviceProvider = serviceProvider;
            _adapterCache = new SimpleQueueAdapterCache(cacheOptions, _name, _loggerFactory);
            _streamQueueMapper = new HashRingBasedStreamQueueMapper(queueMapperOptions.Value, _name);
        }

        public static GrainsQueueAdapterFactory Create(IServiceProvider services, string name)
        {
            var clusterOptions = services.GetProviderClusterOptions(name);
            var cacheOptions = services.GetOptionsByName<SimpleQueueCacheOptions>(name);

            return ActivatorUtilities.CreateInstance<GrainsQueueAdapterFactory>(services, name, cacheOptions, services, clusterOptions);
        }

        public Task<IQueueAdapter> CreateAdapter()
        {
            var adapter = new GrainsQueueAdapter(_serializer, _service, _grainsOptions, _streamQueueMapper, _loggerFactory, _name);

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