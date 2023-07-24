// <copyright file="ITransactionGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;

namespace Orleans.Streaming.Grains.Abstract
{
    public interface ITransactionGrain : IGrainWithStringKey, ITransactionObserver
    {
        Task<Guid?> PopAsync();

        Task PostAsync(Guid id);

        Task<bool> WaitAsync<T>(Guid id);

        Task CompleteAsync(Guid id, bool success);

        Task SubscribeAsync(ITransactionObserver observer);

        Task UnsubscribeAsync(ITransactionObserver observer);

        Task<(Queue<Guid> Queue, Queue<Guid> Poison, Dictionary<Guid, DateTimeOffset> Transactions)> GetStateAsync();
    }
}