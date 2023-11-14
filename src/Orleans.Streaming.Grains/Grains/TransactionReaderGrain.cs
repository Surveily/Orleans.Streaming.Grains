// <copyright file="TransactionReaderGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streaming.Grains.State;

namespace Orleans.Streaming.Grains.Grains
{
    public class TransactionReaderGrain<T> : Grain, ITransactionReaderGrain<T>
    {
        public async Task<Immutable<List<(Guid Id, Immutable<T> Item)>>> GetAsync(List<Guid> ids)
        {
            var results = new List<(Guid Id, Immutable<T> Item)>();

            foreach (var id in ids)
            {
                var itemGrain = GrainFactory.GetGrain<ITransactionItemGrain<T>>(id);
                var item = await itemGrain.GetAsync();

                if (item.Value != null)
                {
                    results.Add((id, item));
                }
            }

            return new Immutable<List<(Guid Id, Immutable<T> Item)>>(results);
        }
    }
}