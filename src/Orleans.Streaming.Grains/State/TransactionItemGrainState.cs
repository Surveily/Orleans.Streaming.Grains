// <copyright file="TransactionItemGrainState.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Orleans;
using Orleans.Concurrency;

namespace Orleans.Streaming.Grains.State
{
    [GenerateSerializer]
    public class TransactionItemGrainState<T>
    {
        [Id(0)]
        public Immutable<T> Item { get; set; }
    }
}