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
    public class TransactionGrain : Grain<TransactionGrainState>, ITransactionGrain
    {
        private readonly GrainsOptions _options;
        private readonly ObserverManager<ITransactionObserver> _subscriptions;

        public TransactionGrain(IOptions<GrainsOptions> options, ILoggerFactory logger)
        {
            _options = options.Value;
            _subscriptions = new ObserverManager<ITransactionObserver>(TimeSpan.FromSeconds(30), logger.CreateLogger<TransactionGrain>());
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if (State.Queue == null)
            {
                State.Queue = new Queue<Guid>();
                State.Poison = new Queue<Guid>();
                State.Transactions = new Dictionary<Guid, TransactionGrainStatePeriod>();

                await PersistAsync();
            }

            var timeout = _options.RetryTimeout / 10;

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

        public Task<List<Guid>> PopAsync(int maxCount)
        {
            var results = new List<Guid>();

            while (results.Count < maxCount && State.Queue.TryDequeue(out var id))
            {
                if (!State.Transactions.ContainsKey(id))
                {
                    State.Transactions.Add(id, new TransactionGrainStatePeriod
                    {
                        Retried = DateTimeOffset.UtcNow,
                        Started = DateTimeOffset.UtcNow,
                    });
                }

                results.Add(id);
            }

            return Task.FromResult(results);
        }

        public Task PostAsync(Guid id)
        {
            State.Queue.Enqueue(id);

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
                        State.Queue.Enqueue(item.Key);
                    }
                }
            }

            await PersistAsync();
        }

        private async Task FlushTimerAsync(object arg)
        {
            await Task.Run(async () => await this.AsReference<ITransactionGrain>().FlushAsync());
        }

        private async Task PersistAsync()
        {
            await WriteStateAsync();
        }
    }
}