// <copyright file="TransactionService.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Utilities;

namespace Orleans.Streaming.Grains.Services
{
    public class TransactionService<T> : ITransactionService<T>
    {
        private readonly IClusterClient _client;
        private readonly ConcurrentDictionary<string, ITransactionGrain<T>> _transactionCache;

        public TransactionService(IClusterClient client)
        {
            _client = client;

            _transactionCache = new ConcurrentDictionary<string, ITransactionGrain<T>>();
        }

        public async Task CompleteAsync(Guid id, bool success, string queue)
        {
            await GetTransactionGrain(queue).CompleteAsync(id, success);

            var item = _client.GetGrain<ITransactionItemGrain<T>>(id);

            await item.DeleteAsync();
        }

        public async Task<(Guid Id, Immutable<T> Item)?> PopAsync(string queue)
        {
            var id = await GetTransactionGrain(queue).PopAsync();

            if (id != null && id.HasValue)
            {
                var item = _client.GetGrain<ITransactionItemGrain<T>>(id.Value);
                var result = await item.GetAsync();

                return (id.Value, result);
            }

            return null;
        }

        public async Task PostAsync(T message, bool wait, string queue)
        {
            var id = Guid.NewGuid();
            var completion = wait ? _client.GetGrain<ITransactionProxyGrain>(id).WaitAsync<T>(queue)
                                  : Task.CompletedTask;

            await GetTransactionGrain(queue).PostAsync(id, message);
            await completion;
        }

        private ITransactionGrain<T> GetTransactionGrain(string queue)
        {
            return _transactionCache.GetOrAdd(queue, _client.GetGrain<ITransactionGrain<T>>(queue));
        }
    }
}