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
using Orleans.Providers;
using Orleans.Providers.Streams.Common;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Streams
{
    public class GrainsQueueAdapterFactory : IQueueAdapterFactory, IQueueAdapterCache
    {
        private readonly string _name;
        private readonly ITransactionService<MemoryMessageData> _service;
        private readonly GrainsOptions _grainsOptions;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IStreamQueueMapper _streamQueueMapper;
        private readonly IMemoryMessageBodySerializer _serializer;
        private readonly StreamCacheEvictionOptions _cacheOptions;
        private readonly StreamStatisticOptions _statisticOptions;

        private TimePurgePredicate _purgePredicate;
        private IObjectPool<FixedSizeBuffer> _bufferPool;
        private BlockPoolMonitorDimensions _blockPoolMonitorDimensions;

        public GrainsQueueAdapterFactory(string name,
                                         IMemoryMessageBodySerializer serializer,
                                         ITransactionService<MemoryMessageData> service,
                                         GrainsOptions grainsOptions,
                                         ILoggerFactory loggerFactory,
                                         IStreamQueueMapper queueMapper,
                                         StreamCacheEvictionOptions cacheOptions,
                                         StreamStatisticOptions statisticOptions)
        {
            _name = name;
            _service = service;
            _serializer = serializer;
            _cacheOptions = cacheOptions;
            _grainsOptions = grainsOptions;
            _loggerFactory = loggerFactory;
            _streamQueueMapper = queueMapper;
            _statisticOptions = statisticOptions;

            _purgePredicate = new TimePurgePredicate(cacheOptions.DataMinTimeInCache, cacheOptions.DataMaxAgeInCache);

            if (_bufferPool == null)
            {
                // 1 meg block size pool
                _blockPoolMonitorDimensions = new BlockPoolMonitorDimensions($"BlockPool-{Guid.NewGuid()}");

                var oneMb = 1 << 20;
                var objectPoolMonitor = new ObjectPoolMonitorBridge(new DefaultBlockPoolMonitor(_blockPoolMonitorDimensions), oneMb);

                _bufferPool = new ObjectPool<FixedSizeBuffer>(() => new FixedSizeBuffer(oneMb), objectPoolMonitor, _statisticOptions.StatisticMonitorWriteInterval);
            }
        }

        public static GrainsQueueAdapterFactory Create(IServiceProvider services, string name)
        {
            var grainsOptions = services.GetOptionsByName<GrainsOptions>(name);
            var queueMapper = services.GetServiceByName<IStreamQueueMapper>(name);
            var statsOptions = services.GetOptionsByName<StreamStatisticOptions>(name);
            var cacheOptions = services.GetOptionsByName<StreamCacheEvictionOptions>(name);
            var queueOptions = services.GetOptionsByName<HashRingStreamQueueMapperOptions>(name);

            return ActivatorUtilities.CreateInstance<GrainsQueueAdapterFactory>(services, name, statsOptions, cacheOptions, grainsOptions, queueMapper ?? new HashRingBasedStreamQueueMapper(queueOptions, name));
        }

        public Task<IQueueAdapter> CreateAdapter()
        {
            var adapter = new GrainsQueueAdapter(_name, _serializer, _grainsOptions, _service, _loggerFactory, _streamQueueMapper);

            return Task.FromResult<IQueueAdapter>(adapter);
        }

        public IQueueCache CreateQueueCache(QueueId queueId)
        {
            var logger = _loggerFactory.CreateLogger($"{typeof(GrainsPooledCache).FullName}.{_name}.{queueId}");
            var monitor = new DefaultCacheMonitor(new CacheMonitorDimensions(queueId.ToString(), _blockPoolMonitorDimensions.BlockPoolId));
            return new GrainsPooledCache(_bufferPool, _purgePredicate, logger, _serializer, monitor, _statisticOptions.StatisticMonitorWriteInterval, _cacheOptions.MetadataMinTimeInCache);
        }

        public Task<IStreamFailureHandler> GetDeliveryFailureHandler(QueueId queueId)
        {
            return Task.FromResult<IStreamFailureHandler>(new NoOpStreamDeliveryFailureHandler());
        }

        public IQueueAdapterCache GetQueueAdapterCache()
        {
            return this;
        }

        public IStreamQueueMapper GetStreamQueueMapper()
        {
            return _streamQueueMapper;
        }
    }
}