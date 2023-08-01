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
        private readonly IEnumerable<QueueId> _queues;
        private readonly IEnumerable<Type> _messageTypes;

        public GrainsQueueMapper(IEnumerable<Type> messageTypes)
        {
            _messageTypes = messageTypes;
            _queues = messageTypes.Select(x => QueueId.GetQueueId(x.Name, 0, 0))
                                  .ToList();
        }

        public IEnumerable<QueueId> GetAllQueues() => _queues;

        public QueueId GetQueueForStream(StreamId streamId) => _queues.Single(x => x.GetStringNamePrefix() == Encoding.UTF8.GetString(streamId.Namespace.ToArray()));
    }
}