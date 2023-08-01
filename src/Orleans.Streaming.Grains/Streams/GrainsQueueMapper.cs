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
        private readonly Dictionary<string, QueueId> _queues;

        public GrainsQueueMapper(IEnumerable<Type> messageTypes)
        {
            _queues = messageTypes.Select(x => QueueId.GetQueueId(x.Name, 0, 0))
                                  .ToDictionary(x => x.GetStringNamePrefix(), x => x);
        }

        public IEnumerable<QueueId> GetAllQueues() => _queues.Values;

        public QueueId GetQueueForStream(StreamId streamId) => _queues[Encoding.UTF8.GetString(streamId.Namespace.Span)];
    }
}