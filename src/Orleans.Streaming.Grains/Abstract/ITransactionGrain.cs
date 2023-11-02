// <copyright file="ITransactionGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Orleans.Streaming.Grains.State;

namespace Orleans.Streaming.Grains.Abstract
{
    public interface ITransactionGrain<T> : IGrainWithStringKey
    {
        Task FlushAsync();

        Task<Guid?> PopAsync();

        Task PostAsync(Guid id, T message);

        Task CompleteAsync(Guid id, bool success);

        Task<TransactionGrainState> GetStateAsync();

        Task SubscribeAsync(ITransactionObserver observer);

        Task UnsubscribeAsync(ITransactionObserver observer);
    }
}