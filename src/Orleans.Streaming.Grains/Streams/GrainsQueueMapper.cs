// <copyright file="GrainsQueueMapper.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System.Text;
using Orleans.Runtime;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Streams
{
    public class GrainsQueueMapper : IStreamQueueMapper
    {
        private static object @lock = new object();

        private readonly Dictionary<string, Queue<QueueId>> _queues;
        private readonly Dictionary<StreamId, QueueId> _pinnedQueues;

        public GrainsQueueMapper(int countEach = 3)
        {
            var messageTypes = AppDomain.CurrentDomain.GetAssemblies()
                                                      .SelectMany(x => x.GetTypes())
                                                      .Where(x => x.GetCustomAttributes(typeof(ImplicitStreamSubscriptionAttribute), true)?.Count() > 0);

            _pinnedQueues = new Dictionary<StreamId, QueueId>();
            _queues = messageTypes.SelectMany(x => Enumerable.Range(0, countEach)
                                                             .Select(y => QueueId.GetQueueId(x.Name, (uint)y, 0)))
                                  .GroupBy(x => x.GetStringNamePrefix())
                                  .ToDictionary(x => x.Key, x => new Queue<QueueId>(x));
        }

        public IEnumerable<QueueId> GetAllQueues() => _queues.Values.SelectMany(x => x);

        public QueueId GetQueueForStream(StreamId streamId)
        {
            lock (@lock)
            {
                if (_pinnedQueues.ContainsKey(streamId))
                {
                    return _pinnedQueues[streamId];
                }

                var queue = _queues[Encoding.UTF8.GetString(streamId.Namespace.Span)];
                var queueId = queue.Dequeue();

                queue.Enqueue(queueId);
                _pinnedQueues[streamId] = queueId;

                return queueId;
            }
        }
    }
}