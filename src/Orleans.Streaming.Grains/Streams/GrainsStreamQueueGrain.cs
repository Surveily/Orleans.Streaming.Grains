// <copyright file="GrainsStreamQueueGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Runtime;

namespace Orleans.Streaming.Grains.Streams
{
    /// <summary>
    /// Memory stream queue grain. This grain works as a storage queue of event data. Enqueue and Dequeue operations are supported.
    /// the max event count sets the max storage limit to the queue.
    /// </summary>
    public class GrainsStreamQueueGrain : Grain, IGrainsStreamQueueGrain, IGrainMigrationParticipant
    {
        /// <summary>
        /// The maximum event count.
        /// </summary>
        private const int MaxEventCount = 16384;

        private Queue<GrainsMessageData> _eventQueue = new Queue<GrainsMessageData>();

        private long _sequenceNumber = DateTime.UtcNow.Ticks;

        public Task Enqueue(GrainsMessageData data)
        {
            if (_eventQueue.Count >= MaxEventCount)
            {
                throw new InvalidOperationException($"Can not enqueue since the count has reached its maximum of {MaxEventCount}");
            }

            data.SequenceNumber = _sequenceNumber++;
            _eventQueue.Enqueue(data);

            return Task.CompletedTask;
        }

        public Task<List<GrainsMessageData>> Dequeue(int maxCount)
        {
            List<GrainsMessageData> list = new List<GrainsMessageData>();

            for (int i = 0; i < maxCount && _eventQueue.Count > 0; ++i)
            {
                list.Add(_eventQueue.Dequeue());
            }

            return Task.FromResult(list);
        }

        public void OnDehydrate(IDehydrationContext dehydrationContext)
        {
            dehydrationContext.TryAddValue("queue", _eventQueue);
        }

        public void OnRehydrate(IRehydrationContext rehydrationContext)
        {
            if (rehydrationContext.TryGetValue("queue", out Queue<GrainsMessageData> value))
            {
                _eventQueue = value;
            }
        }
    }
}
