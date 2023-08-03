// <copyright file="TransactionGrainState.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;

namespace Orleans.Streaming.Grains.State
{
    [GenerateSerializer]
    public class TransactionGrainState
    {
        [Id(0)]
        public Queue<Guid> Queue { get; set; }

        [Id(1)]
        public Queue<Guid> Poison { get; set; }

        [Id(2)]
        public Dictionary<Guid, DateTimeOffset> Transactions { get; set; }

        [Id(3)]
        public Dictionary<Guid, int> TransactionCounts { get; set; }
    }
}