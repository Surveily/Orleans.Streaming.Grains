// <copyright file="GrainsPooledCache.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using Orleans.Providers;
using Orleans.Providers.Streams.Common;
using Orleans.Runtime;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Streams
{
    public class GrainsPooledCache<TSerializer> : IQueueCache, ICacheDataAdapter
        where TSerializer : class, IMemoryMessageBodySerializer
    {
        private readonly IObjectPool<FixedSizeBuffer> _bufferPool;
        private readonly TSerializer _serializer;
        private readonly IEvictionStrategy _evictionStrategy;
        private readonly PooledQueueCache _cache;

        private FixedSizeBuffer _currentBuffer;

        public GrainsPooledCache(
            IObjectPool<FixedSizeBuffer> bufferPool,
            TimePurgePredicate purgePredicate,
            ILogger logger,
            TSerializer serializer,
            ICacheMonitor cacheMonitor,
            TimeSpan? monitorWriteInterval,
            TimeSpan? purgeMetadataInterval)
        {
            _bufferPool = bufferPool;
            _serializer = serializer;
            _cache = new PooledQueueCache(this, logger, cacheMonitor, monitorWriteInterval, purgeMetadataInterval);
            _evictionStrategy = new ChronologicalEvictionStrategy(logger, purgePredicate, cacheMonitor, monitorWriteInterval) { PurgeObservable = _cache };
        }

        /// <inheritdoc/>
        public int GetMaxAddCount()
        {
            return 100;
        }

        /// <inheritdoc/>
        public void AddToCache(IList<IBatchContainer> messages)
        {
            DateTime utcNow = DateTime.UtcNow;
            List<CachedMessage> memoryMessages = messages
                .Cast<GrainsBatchContainer<TSerializer>>()
                .Select(container => container.MessageData)
                .Select(batch => QueueMessageToCachedMessage(batch, utcNow))
                .ToList();
            _cache.Add(memoryMessages, DateTime.UtcNow);
        }

        /// <inheritdoc/>
        public bool TryPurgeFromCache(out IList<IBatchContainer> purgedItems)
        {
            purgedItems = null;
            _evictionStrategy.PerformPurge(DateTime.UtcNow);
            return false;
        }

        /// <inheritdoc/>
        public IQueueCacheCursor GetCacheCursor(StreamId streamId, StreamSequenceToken token)
        {
            return new Cursor(_cache, streamId, token);
        }

        /// <inheritdoc/>
        public bool IsUnderPressure()
        {
            return false;
        }

        /// <inheritdoc/>
        public IBatchContainer GetBatchContainer(ref CachedMessage cachedMessage)
        {
            int readOffset = 0;
            ArraySegment<byte> payload = SegmentBuilder.ReadNextBytes(cachedMessage.Segment, ref readOffset);
            GrainsMessageData message = GrainsMessageData.Create(cachedMessage.StreamId, new ArraySegment<byte>(payload.ToArray()));
            message.SequenceNumber = cachedMessage.SequenceNumber;
            return new GrainsBatchContainer<TSerializer>(message, _serializer);
        }

        /// <inheritdoc/>
        public StreamSequenceToken GetSequenceToken(ref CachedMessage cachedMessage)
        {
            return new EventSequenceToken(cachedMessage.SequenceNumber);
        }

        private CachedMessage QueueMessageToCachedMessage(GrainsMessageData queueMessage, DateTime dequeueTimeUtc)
        {
            StreamPosition streamPosition = GetStreamPosition(queueMessage);
            return new CachedMessage()
            {
                StreamId = streamPosition.StreamId,
                SequenceNumber = queueMessage.SequenceNumber,
                EnqueueTimeUtc = queueMessage.EnqueueTimeUtc,
                DequeueTimeUtc = dequeueTimeUtc,
                Segment = SerializeMessageIntoPooledSegment(queueMessage)
            };
        }

        private ArraySegment<byte> SerializeMessageIntoPooledSegment(GrainsMessageData queueMessage)
        {
            int size = SegmentBuilder.CalculateAppendSize(queueMessage.Payload);

            ArraySegment<byte> segment;

            if (_currentBuffer == null || !_currentBuffer.TryGetSegment(size, out segment))
            {
                _currentBuffer = _bufferPool.Allocate();
                _evictionStrategy.OnBlockAllocated(_currentBuffer);

                if (!_currentBuffer.TryGetSegment(size, out segment))
                {
                    string errmsg = string.Format(CultureInfo.InvariantCulture,
                        "Message size is too big. MessageSize: {0}", size);
                    throw new ArgumentOutOfRangeException(nameof(queueMessage), errmsg);
                }
            }

            int writeOffset = 0;
            SegmentBuilder.Append(segment, ref writeOffset, queueMessage.Payload);
            return segment;
        }

        private StreamPosition GetStreamPosition(GrainsMessageData queueMessage)
        {
            return new StreamPosition(queueMessage.StreamId,
                new EventSequenceTokenV2(queueMessage.SequenceNumber));
        }

        private class Cursor : IQueueCacheCursor
        {
            private readonly PooledQueueCache _cache;
            private readonly object _cursor;
            private IBatchContainer _current;

            public Cursor(PooledQueueCache cache, StreamId streamId, StreamSequenceToken token)
            {
                _cache = cache;
                _cursor = cache.GetCursor(streamId, token);
            }

            public void Dispose()
            {
            }

            public IBatchContainer GetCurrent(out Exception exception)
            {
                exception = null;

                return _current;
            }

            public bool MoveNext()
            {
                IBatchContainer next;

                if (!_cache.TryGetNextMessage(_cursor, out next))
                {
                    return false;
                }

                _current = next;

                return true;
            }

            public void Refresh(StreamSequenceToken token)
            {
            }

            public void RecordDeliveryFailure()
            {
            }
        }
    }
}
