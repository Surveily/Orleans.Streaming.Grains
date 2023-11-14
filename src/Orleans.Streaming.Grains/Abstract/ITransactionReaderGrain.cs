// <copyright file="ITransactionReaderGrain.cs" company="Surveily Sp. z o.o.">
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
    public interface ITransactionReaderGrain<T> : IGrainWithStringKey
    {
        Task<Immutable<List<(Guid Id, Immutable<T> Item, long Sequence)>>> GetAsync(List<(Guid, long)> ids);
    }
}