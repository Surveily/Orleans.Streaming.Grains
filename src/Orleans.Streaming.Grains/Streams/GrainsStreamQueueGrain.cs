// <copyright file="GrainsStreamQueueGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streaming.Grains.State;
using Orleans.Utilities;

namespace Orleans.Streaming.Grains.Streams
{
    public class GrainsStreamQueueGrain : Grain, IGrainsStreamQueueGrain, IGrainMigrationParticipant
    {
        private const int MaxEventCount = 16384;

        private readonly IOptions<GrainsOptions> _options;
        private readonly ObserverManager<ITransactionObserver> _subscriptions;

        private long _sequenceNumber;
        private Queue<GrainsMessageData> _eventQueue;
        private Dictionary<Guid, TransactionGrainStatePeriod> _transactions;

        public GrainsStreamQueueGrain(IOptions<GrainsOptions> options, ILoggerFactory logger)
        {
            _options = options;
            _sequenceNumber = DateTime.UtcNow.Ticks;
            _eventQueue = new Queue<GrainsMessageData>();
            _transactions = new Dictionary<Guid, TransactionGrainStatePeriod>();
            _subscriptions = new ObserverManager<ITransactionObserver>(TimeSpan.FromSeconds(30), logger.CreateLogger<GrainsStreamQueueGrain>());
        }

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

        public async Task CompleteAsync(Guid id, bool success)
        {
            if (_transactions.Remove(id, out _) /*|| State.Poison.Contains(id)*/)
            {
                /*if (!success)
                {
                    State.Poison.Enqueue(id);
                }*/

                if (_subscriptions.Any())
                {
                    await _subscriptions.Notify(x => x.CompletedAsync(id, success, this.GetPrimaryKeyString()));
                }
            }
        }

        public Task SubscribeAsync(ITransactionObserver observer)
        {
            _subscriptions.Subscribe(observer, observer);

            return Task.CompletedTask;
        }

        public Task UnsubscribeAsync(ITransactionObserver observer)
        {
            _subscriptions.Unsubscribe(observer);

            return Task.CompletedTask;
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
