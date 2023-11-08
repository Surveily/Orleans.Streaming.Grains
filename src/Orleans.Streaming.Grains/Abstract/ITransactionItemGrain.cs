// <copyright file="ITransactionItemGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace Orleans.Streaming.Grains.Abstract
{
    public interface ITransactionItemGrain<T> : IGrainWithIntegerKey
    {
        Task<Immutable<T>> GetAsync();

        Task SetAsync(Immutable<T> item);

        Task DeleteAsync();

        Task PersistAsync();
    }
}