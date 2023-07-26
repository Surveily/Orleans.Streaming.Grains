// <copyright file="TransactionProxyGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Orleans.Streaming.Grains.Abstract;

namespace Orleans.Streaming.Grains.Grains
{
    [Reentrant]
    public class TransactionProxyGrain : Grain, ITransactionProxyGrain
    {
        private readonly TaskCompletionSource<bool> _task;

        private bool _isDisposed;

        public TransactionProxyGrain()
        {
            _task = new TaskCompletionSource<bool>();
        }

        public Task Task => _task.Task;

        public Task CompletedAsync(Guid id, bool success)
        {
            if (id == this.GetPrimaryKey())
            {
                _task.SetResult(success);
            }

            return Task.CompletedTask;
        }

        public async Task<bool> WaitAsync<T>(string queue)
        {
            var transaction = GrainFactory.GetGrain<ITransactionGrain>(queue);
            await transaction.SubscribeAsync(this.AsReference<ITransactionObserver>());
            return await _task.Task;
        }
    }
}