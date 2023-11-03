// <copyright file="ITransactionService.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Concurrency;

namespace Orleans.Streaming.Grains.Abstract
{
    public interface ITransactionService<T>
    {
        Task<(Guid Id, Immutable<T> Item, long Sequence)?> PopAsync(string queue);

        Task PostAsync(Immutable<T> message, bool wait, string queue);

        Task CompleteAsync(Guid id, bool success, string queue);
    }
}