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
        Task<Immutable<T>?> PopAsync<T>(string queue);

        Task PostAsync<T>(Immutable<T> message, bool wait, string queue);

        Task CompleteAsync<T>(long id, bool success, string queue);
    }
}