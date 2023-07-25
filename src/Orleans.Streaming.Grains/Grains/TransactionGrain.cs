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
    [Reentrant]
    public class TransactionGrain : Grain<TransactionGrainState>, ITransactionGrain, IDisposable
    {
        private readonly SemaphoreSlim _lock;
        private readonly GrainsOptions _options;
        private readonly ObserverManager<ITransactionObserver> _subscriptions;
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<bool>> _subscriptionTasks;

        private bool _isDisposed;

        public TransactionGrain(IOptions<GrainsOptions> options, ILoggerFactory logger)
        {
            _options = options.Value;
            _lock = new SemaphoreSlim(1, 1);
            _subscriptionTasks = new ConcurrentDictionary<Guid, TaskCompletionSource<bool>>();
            _subscriptions = new ObserverManager<ITransactionObserver>(TimeSpan.FromMinutes(5), logger.CreateLogger<TransactionGrain>());
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if (State.Queue == null)
            {
                State.Queue = new Queue<Guid>();
                State.Poison = new Queue<Guid>();
                State.Transactions = new Dictionary<Guid, DateTimeOffset>();

                await PersistAsync();
            }

            var timeout = _options.Timeout / 5;

            _ = RegisterTimer(FlushAsync, null, timeout, timeout);

            await base.OnActivateAsync(cancellationToken);
        }

        public async Task CompleteAsync(Guid id, bool success)
        {
            if (State.Transactions.Remove(id, out _))
            {
                if (!success)
                {
                    State.Poison.Enqueue(id);
                }

                await PersistAsync();

                await _subscriptions.Notify(x => x.CompletedAsync(id, success));
            }
        }

        public Task CompletedAsync(Guid id, bool success)
        {
            if (_subscriptionTasks.TryRemove(id, out var task))
            {
                task.SetResult(success);
            }

            return Task.CompletedTask;
        }

        public async Task<Guid?> PopAsync()
        {
            if (State.Queue.TryDequeue(out var id))
            {
                State.Transactions.Add(id, DateTimeOffset.UtcNow);

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

        public Task<(Queue<Guid> Queue, Queue<Guid> Poison, Dictionary<Guid, DateTimeOffset> Transactions)> GetStateAsync()
        {
            return Task.FromResult((State.Queue, State.Poison, State.Transactions));
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

        public async Task<bool> WaitAsync<T>(Guid id)
        {
            var task = _subscriptionTasks.GetOrAdd(id, x => new TaskCompletionSource<bool>());

            await SubscribeAsync(this);

            return await task.Task;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _lock.Dispose();
                }

                _isDisposed = true;
            }
        }

        private async Task FlushAsync(object arg)
        {
            var expired = State.Transactions.Where(x => (DateTimeOffset.UtcNow - x.Value) > _options.Timeout)
                                            .ToList();

            if (expired.Any())
            {
                foreach (var item in expired)
                {
                    State.Queue.Enqueue(item.Key);
                    State.Transactions.Remove(item.Key);
                }

                await PersistAsync();
            }
        }

        private async Task PersistAsync()
        {
            var wait = !_options.FireAndForgetDelivery;

            if (wait)
            {
                try
                {
                    await _lock.WaitAsync();

                    await WriteStateAsync();
                }
                finally
                {
                    _lock.Release();
                }
            }
            else
            {
                await WriteStateAsync();
            }
        }
    }
}