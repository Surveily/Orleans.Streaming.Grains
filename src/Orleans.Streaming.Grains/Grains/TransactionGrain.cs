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
            _subscriptions = new ObserverManager<ITransactionObserver>(TimeSpan.FromMinutes(5), logger.CreateLogger<TransactionGrain>());
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if (State.Queue == null)
            {
                State.Queue = new Queue<Guid>();
                State.Poison = new Queue<Guid>();
                State.TransactionCounts = new Dictionary<Guid, int>();
                State.Transactions = new Dictionary<Guid, DateTimeOffset>();

                await PersistAsync();
            }

            var timeout = _options.Timeout / 5;

            _ = RegisterTimer(FlushTimerAsync, null, timeout, timeout);

            await base.OnActivateAsync(cancellationToken);
        }

        public async Task CompleteAsync(Guid id, bool success)
        {
            if (State.Transactions.Remove(id, out _))
            {
                State.TransactionCounts.Remove(id, out _);

                if (!success)
                {
                    State.Poison.Enqueue(id);
                }

                await PersistAsync();

                if (_subscriptions.Any())
                {
                    await _subscriptions.Notify(x => x.CompletedAsync(id, success, this.GetPrimaryKeyString()));
                }
            }
        }

        public async Task<Guid?> PopAsync()
        {
            if (State.Queue.TryDequeue(out var id))
            {
                State.Transactions.Add(id, DateTimeOffset.UtcNow);

                if (!State.TransactionCounts.ContainsKey(id))
                {
                    State.TransactionCounts[id] = 0;
                }

                State.TransactionCounts[id]++;

                await PersistAsync();

                return id;
            }

            return null;
        }

        public async Task PostAsync(Guid id)
        {
            State.Queue.Enqueue(id);

            await PersistAsync();
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
            var expired = State.Transactions.Where(x => (DateTimeOffset.UtcNow - x.Value) > _options.Timeout)
                                            .ToList();

            if (expired.Any())
            {
                foreach (var item in expired)
                {
                    if (State.TransactionCounts[item.Key] > _options.Retry)
                    {
                        await CompleteAsync(item.Key, false);
                    }
                    else
                    {
                        State.Queue.Enqueue(item.Key);
                        State.Transactions.Remove(item.Key);
                    }
                }

                await PersistAsync();
            }
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