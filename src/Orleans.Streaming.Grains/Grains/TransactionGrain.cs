// <copyright file="TransactionGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Concurrency;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streaming.Grains.State;
using Orleans.Streaming.Grains.Streams;
using Orleans.Utilities;

namespace Orleans.Streaming.Grains.Grains
{
    public class TransactionGrain<T> : Grain<TransactionGrainState>, ITransactionGrain<T>
    {
        private readonly GrainsOptions _options;
        private readonly ObserverManager<ITransactionObserver> _subscriptions;

        private long _sequenceNumber = DateTime.UtcNow.Ticks;

        public TransactionGrain(IOptions<GrainsOptions> options, ILoggerFactory logger)
        {
            _options = options.Value;
            _subscriptions = new ObserverManager<ITransactionObserver>(TimeSpan.FromSeconds(30), logger.CreateLogger<TransactionGrain<T>>());
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if (State.Queue == null)
            {
                State.Poison = new Queue<Guid>();
                State.Queue = new Queue<(Guid, long)>();
                State.Transactions = new Dictionary<Guid, TransactionGrainStatePeriod>();

                await PersistAsync();
            }

            var timeout = _options.RetryTimeout / 3;

            _ = RegisterTimer(FlushTimerAsync, null, timeout, timeout);

            await base.OnActivateAsync(cancellationToken);
        }

        public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            await PersistAsync();

            await base.OnDeactivateAsync(reason, cancellationToken);
        }

        public async Task CompleteAsync(Guid id, bool success)
        {
            if (State.Transactions.Remove(id, out _) || State.Poison.Contains(id))
            {
                if (!success)
                {
                    State.Poison.Enqueue(id);
                }

                if (_subscriptions.Any())
                {
                    await _subscriptions.Notify(x => x.CompletedAsync(id, success, this.GetPrimaryKeyString()));
                }
            }
        }

        public Task<List<(Guid, long)>> PopAsync(int maxCount)
        {
            var results = new List<(Guid, long)>();

            while (results.Count < maxCount && State.Queue.TryDequeue(out var key))
            {
                var (id, _) = key;

                if (!State.Transactions.ContainsKey(id))
                {
                    State.Transactions.Add(id, new TransactionGrainStatePeriod
                    {
                        Retried = DateTimeOffset.UtcNow,
                        Started = DateTimeOffset.UtcNow,
                    });
                }

                results.Add(key);
            }

            return Task.FromResult(results);
        }

        public Task PostAsync(Guid id)
        {
            State.Queue.Enqueue((id, _sequenceNumber++));

            return Task.CompletedTask;
        }

        public Task<TransactionGrainState> GetStateAsync()
        {
            return Task.FromResult(State);
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

        public async Task FlushAsync()
        {
            var expired = State.Transactions.Where(x => (DateTimeOffset.UtcNow - x.Value.Retried) > _options.RetryTimeout)
                                            .ToList();

            if (expired.Any())
            {
                foreach (var item in expired)
                {
                    if (DateTimeOffset.UtcNow - State.Transactions[item.Key].Started > _options.PoisonTimeout)
                    {
                        await CompleteAsync(item.Key, false);
                    }
                    else
                    {
                        State.Transactions[item.Key].Retried = DateTimeOffset.UtcNow;
                        State.Queue.Enqueue((item.Key, _sequenceNumber++));
                    }
                }
            }

            await PersistAsync();
        }

        private async Task FlushTimerAsync(object arg)
        {
            await this.AsReference<ITransactionGrain<T>>().FlushAsync();
        }

        private async Task PersistAsync()
        {
            await WriteStateAsync();
        }
    }
}