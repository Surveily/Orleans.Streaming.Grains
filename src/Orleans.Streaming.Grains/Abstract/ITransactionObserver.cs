// <copyright file="ITransactionObserver.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Streaming.Grains.Abstract
{
    public interface ITransactionObserver : IGrainObserver
    {
        Task CompletedAsync(Guid id, bool success);
    }
}