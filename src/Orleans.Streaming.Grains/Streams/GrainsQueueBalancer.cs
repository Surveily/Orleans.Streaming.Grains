// <copyright file="GrainsQueueBalancer.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Streams
{
    public class GrainsQueueBalancer : IStreamQueueBalancer
    {
        private readonly List<QueueId> _queues;

        public GrainsQueueBalancer()
        {
            _queues = new List<QueueId>();
        }

        public IEnumerable<QueueId> GetMyQueues() => _queues;

        public Task Initialize(IStreamQueueMapper queueMapper)
        {
            _queues.AddRange(queueMapper.GetAllQueues());

            return Task.CompletedTask;
        }

        public Task Shutdown() => Task.CompletedTask;

        public bool SubscribeToQueueDistributionChangeEvents(IStreamQueueBalanceListener observer) => false;

        public bool UnSubscribeFromQueueDistributionChangeEvents(IStreamQueueBalanceListener observer) => false;
    }
}