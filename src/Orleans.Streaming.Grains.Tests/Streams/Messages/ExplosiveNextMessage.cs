// <copyright file="ExplosiveNextMessage.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Concurrency;

namespace Orleans.Streaming.Grains.Tests.Streams.Messages
{
    [GenerateSerializer]
    public class ExplosiveNextMessage
    {
        [Id(0)]
        public Immutable<byte[]> Data { get; set; }

        [Id(1)]
        public Immutable<string> Text { get; set; }
    }
}