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
    public interface ITransactionService
    {
        Task<List<(Guid Id, Immutable<T> Item)>> PopAsync<T>(string queue, int maxCount);

        Task PostAsync<T>(Immutable<T> message, bool wait, string queue);

        Task CompleteAsync<T>(Guid id, bool success, string queue);
    }
}