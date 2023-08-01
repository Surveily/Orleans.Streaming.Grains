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
using Orleans.Runtime;
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
        private readonly GrainsOptions _grainsOptions;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IQueueAdapterCache _adapterCache;
        private readonly IStreamQueueMapper _streamQueueMapper;

        public GrainsQueueAdapterFactory(string name,
                                         Serializer serializer,
                                         ITransactionService service,
                                         GrainsOptions grainsOptions,
                                         ILoggerFactory loggerFactory,
                                         IStreamQueueMapper queueMapper,
                                         SimpleQueueCacheOptions cacheOptions)
        {
            _name = name;
            _service = service;
            _serializer = serializer;
            _grainsOptions = grainsOptions;
            _loggerFactory = loggerFactory;
            _streamQueueMapper = queueMapper;
            _adapterCache = new SimpleQueueAdapterCache(cacheOptions, _name, _loggerFactory);
        }

        public static GrainsQueueAdapterFactory Create(IServiceProvider services, string name)
        {
            var grainsOptions = services.GetOptionsByName<GrainsOptions>(name);
            var queueMapper = services.GetServiceByName<IStreamQueueMapper>(name);
            var cacheOptions = services.GetOptionsByName<SimpleQueueCacheOptions>(name);
            var queueOptions = services.GetOptionsByName<HashRingStreamQueueMapperOptions>(name);

            return ActivatorUtilities.CreateInstance<GrainsQueueAdapterFactory>(services, name, cacheOptions, grainsOptions, queueMapper ?? new HashRingBasedStreamQueueMapper(queueOptions, name));
        }

        public Task<IQueueAdapter> CreateAdapter()
        {
            var adapter = new GrainsQueueAdapter(_name, _serializer, _grainsOptions, _service, _streamQueueMapper);

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