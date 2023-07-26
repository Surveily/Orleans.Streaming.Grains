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
    public class TransactionService : ITransactionService
    {
        private readonly IClusterClient _client;

        public TransactionService(IClusterClient client)
        {
            _client = client;
        }

        public async Task CompleteAsync<T>(Guid id, bool success, string queue)
        {
            var transaction = _client.GetGrain<ITransactionGrain>(queue);

            await transaction.CompleteAsync(id, success);

            var item = _client.GetGrain<ITransactionItemGrain<T>>(id);

            await item.DeleteAsync();
        }

        public async Task<(Guid Id, Immutable<T> Item)?> PopAsync<T>(string queue)
        {
            var transaction = _client.GetGrain<ITransactionGrain>(queue);
            var id = await transaction.PopAsync();

            if (id != null)
            {
                var item = _client.GetGrain<ITransactionItemGrain<T>>(id.Value);

                return (id.Value, await item.GetAsync());
            }

            return null;
        }

        public async Task PostAsync<T>(Immutable<T> message, bool wait, string queue)
        {
            var id = Guid.NewGuid();
            var item = _client.GetGrain<ITransactionItemGrain<T>>(id);
            await item.SetAsync(message);

            var transaction = _client.GetGrain<ITransactionGrain>(queue);
            var completion = wait ? _client.GetGrain<ITransactionProxyGrain>(id).WaitAsync<T>(queue)
                                  : Task.CompletedTask;

            await transaction.PostAsync(id);
            await completion;
        }
    }
}