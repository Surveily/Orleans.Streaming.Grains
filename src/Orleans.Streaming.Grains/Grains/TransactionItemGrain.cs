// <copyright file="TransactionItemGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streaming.Grains.State;

namespace Orleans.Streaming.Grains.Grains
{
    public class TransactionItemGrain<T> : Grain<TransactionItemGrainState<T>>, ITransactionItemGrain<T>
    {
        private bool _deleted;

        public async Task DeleteAsync()
        {
            await ClearStateAsync();

            _deleted = true;
        }

        public Task<Immutable<T>> GetAsync()
        {
            if (!_deleted)
            {
                return Task.FromResult(State.Item);
            }

            return Task.FromResult(new Immutable<T>(default));
        }

        public Task SetAsync(Immutable<T> item)
        {
            if (_deleted)
            {
                throw new DataException("Cannot set state of a deleted TransactionItem.");
            }

            State.Item = item;

            _ = RegisterTimer(PersistTimerAsync, null, TimeSpan.FromSeconds(1), TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        public async Task PersistAsync()
        {
            if (!_deleted)
            {
                await WriteStateAsync();
            }
        }

        private async Task PersistTimerAsync(object arg)
        {
            await Task.Run(async () => await this.AsReference<ITransactionItemGrain<T>>().PersistAsync());
        }
    }
}