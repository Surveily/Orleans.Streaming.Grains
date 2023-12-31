// <copyright file="ITransactionProxyGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Streaming.Grains.Abstract
{
    public interface ITransactionProxyGrain : IGrainWithGuidKey, ITransactionObserver
    {
        Task<bool> WaitAsync<T>(string queue);
    }
}