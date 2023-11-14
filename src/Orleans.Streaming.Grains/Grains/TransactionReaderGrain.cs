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
        public async Task<Immutable<List<(Guid Id, Immutable<T> Item, long Sequence)>>> GetAsync(List<(Guid, long)> ids)
        {
            var results = new List<(Guid, Immutable<T>, long)>();

            foreach (var (id, sequence) in ids)
            {
                var itemGrain = GrainFactory.GetGrain<ITransactionItemGrain<T>>(id);
                var item = await itemGrain.GetAsync();

                results.Add((id, item, sequence));
            }

            return new Immutable<List<(Guid, Immutable<T>, long)>>(results);
        }
    }
}