// <copyright file="GrainsAdapterFactory.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Providers;
using Orleans.Providers.Streams.Common;
using Orleans.Runtime;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Streams
{
    public class GrainsAdapterFactory<TSerializer> : IQueueAdapterFactory, IQueueAdapter, IQueueAdapterCache
        where TSerializer : class, IMemoryMessageBodySerializer
    {
        protected Func<CacheMonitorDimensions, ICacheMonitor> cacheMonitorFactory;
        protected Func<BlockPoolMonitorDimensions, IBlockPoolMonitor> blockPoolMonitorFactory;
        protected Func<ReceiverMonitorDimensions, IQueueAdapterReceiverMonitor> receiverMonitorFactory;

        private readonly StreamCacheEvictionOptions _cacheOptions;
        private readonly StreamStatisticOptions _statisticOptions;
        private readonly HashRingStreamQueueMapperOptions _queueMapperOptions;
        private readonly IGrainFactory _grainFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly TSerializer _serializer;
        private readonly ulong _nameHash;
        private IStreamQueueMapper _streamQueueMapper;
        private ConcurrentDictionary<QueueId, IGrainsStreamQueueGrain> _queueGrains;
        private IObjectPool<FixedSizeBuffer> _bufferPool;
        private BlockPoolMonitorDimensions _blockPoolMonitorDimensions;
        private IStreamFailureHandler _streamFailureHandler;
        private TimePurgePredicate _purgePredicate;

        public GrainsAdapterFactory(
            string providerName,
            StreamCacheEvictionOptions cacheOptions,
            StreamStatisticOptions statisticOptions,
            HashRingStreamQueueMapperOptions queueMapperOptions,
            IServiceProvider serviceProvider,
            IGrainFactory grainFactory,
            ILoggerFactory loggerFactory)
        {
            Name = providerName;
            _queueMapperOptions = queueMapperOptions ?? throw new ArgumentNullException(nameof(queueMapperOptions));
            _cacheOptions = cacheOptions ?? throw new ArgumentNullException(nameof(cacheOptions));
            _statisticOptions = statisticOptions ?? throw new ArgumentException(nameof(statisticOptions));
            _grainFactory = grainFactory ?? throw new ArgumentNullException(nameof(grainFactory));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<ILogger<GrainsAdapterFactory<TSerializer>>>();
            _serializer = GrainsMessageBodySerializerFactory<TSerializer>.GetOrCreateSerializer(serviceProvider);

            var nameBytes = BitConverter.IsLittleEndian ? MemoryMarshal.AsBytes(Name.AsSpan()) : Encoding.Unicode.GetBytes(Name);
            XxHash64.Hash(nameBytes, MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref _nameHash, 1)));
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public bool IsRewindable => true;

        /// <inheritdoc />
        public StreamProviderDirection Direction => StreamProviderDirection.ReadWrite;

        protected Func<string, Task<IStreamFailureHandler>> StreamFailureHandlerFactory { get; set; }

        /// <summary>
        /// Creates a new <see cref="GrainsAdapterFactory{TSerializer}"/> instance.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="name">The provider name.</param>
        /// <returns>A mew <see cref="GrainsAdapterFactory{TSerializer}"/> instance.</returns>
        public static GrainsAdapterFactory<TSerializer> Create(IServiceProvider services, string name)
        {
            var cachePurgeOptions = services.GetOptionsByName<StreamCacheEvictionOptions>(name);
            var statisticOptions = services.GetOptionsByName<StreamStatisticOptions>(name);
            var queueMapperOptions = services.GetOptionsByName<HashRingStreamQueueMapperOptions>(name);
            var factory = ActivatorUtilities.CreateInstance<GrainsAdapterFactory<TSerializer>>(services, name, cachePurgeOptions, statisticOptions, queueMapperOptions);
            factory.Init();
            return factory;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Init()
        {
            _queueGrains = new ConcurrentDictionary<QueueId, IGrainsStreamQueueGrain>();

            if (cacheMonitorFactory == null)
            {
                cacheMonitorFactory = (dimensions) => new DefaultCacheMonitor(dimensions);
            }

            if (blockPoolMonitorFactory == null)
            {
                blockPoolMonitorFactory = (dimensions) => new DefaultBlockPoolMonitor(dimensions);
            }

            if (receiverMonitorFactory == null)
            {
                receiverMonitorFactory = (dimensions) => new DefaultQueueAdapterReceiverMonitor(dimensions);
            }

            _purgePredicate = new TimePurgePredicate(_cacheOptions.DataMinTimeInCache, _cacheOptions.DataMaxAgeInCache);
            _streamQueueMapper = new HashRingBasedStreamQueueMapper(_queueMapperOptions, Name);
        }

        /// <inheritdoc />
        public Task<IQueueAdapter> CreateAdapter()
        {
            return Task.FromResult<IQueueAdapter>(this);
        }

        /// <inheritdoc />
        public IQueueAdapterCache GetQueueAdapterCache()
        {
            return this;
        }

        /// <inheritdoc />
        public IStreamQueueMapper GetStreamQueueMapper()
        {
            return _streamQueueMapper;
        }

        /// <inheritdoc />
        public IQueueAdapterReceiver CreateReceiver(QueueId queueId)
        {
            var dimensions = new ReceiverMonitorDimensions(queueId.ToString());
            var receiverLogger = _loggerFactory.CreateLogger($"{typeof(GrainsAdapterReceiver<TSerializer>).FullName}.{Name}.{queueId}");
            var receiverMonitor = receiverMonitorFactory(dimensions);
            IQueueAdapterReceiver receiver = new GrainsAdapterReceiver<TSerializer>(GetQueueGrain(queueId), receiverLogger, _serializer, receiverMonitor);
            return receiver;
        }

        /// <inheritdoc />
        public async Task QueueMessageBatchAsync<T>(StreamId streamId, IEnumerable<T> events, StreamSequenceToken token, Dictionary<string, object> requestContext)
        {
            try
            {
                var queueId = _streamQueueMapper.GetQueueForStream(streamId);
                ArraySegment<byte> bodyBytes = _serializer.Serialize(new MemoryMessageBody(events.Cast<object>(), requestContext));
                var messageData = GrainsMessageData.Create(streamId, bodyBytes);
                IGrainsStreamQueueGrain queueGrain = GetQueueGrain(queueId);
                await queueGrain.Enqueue(messageData);
            }
            catch (Exception exc)
            {
                _logger.LogError((int)ProviderErrorCode.MemoryStreamProviderBase_QueueMessageBatchAsync, exc, "Exception thrown in MemoryAdapterFactory.QueueMessageBatchAsync.");
                throw;
            }
        }

        /// <inheritdoc />
        public IQueueCache CreateQueueCache(QueueId queueId)
        {
            CreateBufferPoolIfNotCreatedYet();
            var logger = _loggerFactory.CreateLogger($"{typeof(GrainsPooledCache<TSerializer>).FullName}.{Name}.{queueId}");
            var monitor = cacheMonitorFactory(new CacheMonitorDimensions(queueId.ToString(), _blockPoolMonitorDimensions.BlockPoolId));
            return new GrainsPooledCache<TSerializer>(_bufferPool, _purgePredicate, logger, _serializer, monitor, _statisticOptions.StatisticMonitorWriteInterval, _cacheOptions.MetadataMinTimeInCache);
        }

        /// <inheritdoc />
        public Task<IStreamFailureHandler> GetDeliveryFailureHandler(QueueId queueId)
        {
            return Task.FromResult(_streamFailureHandler ?? (_streamFailureHandler = new NoOpStreamDeliveryFailureHandler()));
        }

        private void CreateBufferPoolIfNotCreatedYet()
        {
            if (_bufferPool == null)
            {
                // 1 meg block size pool
                _blockPoolMonitorDimensions = new BlockPoolMonitorDimensions($"BlockPool-{Guid.NewGuid()}");
                var oneMb = 1 << 20;
                var objectPoolMonitor = new ObjectPoolMonitorBridge(blockPoolMonitorFactory(_blockPoolMonitorDimensions), oneMb);
                _bufferPool = new ObjectPool<FixedSizeBuffer>(() => new FixedSizeBuffer(oneMb), objectPoolMonitor, _statisticOptions.StatisticMonitorWriteInterval);
            }
        }

        /// <summary>
        /// Generate a deterministic Guid from a queue Id.
        /// </summary>
        private Guid GenerateDeterministicGuid(QueueId queueId)
        {
            Span<byte> bytes = stackalloc byte[16];
            MemoryMarshal.Write(bytes, ref Unsafe.AsRef(in _nameHash));
            BinaryPrimitives.WriteUInt32LittleEndian(bytes[8..], queueId.GetUniformHashCode());
            BinaryPrimitives.WriteUInt32LittleEndian(bytes[12..], queueId.GetNumericId());

            return new Guid(bytes);
        }

        /// <summary>
        /// Get a MemoryStreamQueueGrain instance by queue Id.
        /// </summary>
        private IGrainsStreamQueueGrain GetQueueGrain(QueueId queueId)
        {
            return _queueGrains.GetOrAdd(queueId, (id, arg) => arg._grainFactory.GetGrain<IGrainsStreamQueueGrain>(arg.instance.GenerateDeterministicGuid(id)), (instance: this, _grainFactory));
        }
    }
}
