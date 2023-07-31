// <copyright file="GrainsMessage.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Runtime;

namespace Orleans.Streaming.Grains.Streams
{
    [GenerateSerializer]
    public class GrainsMessage
    {
        [Id(0)]
        public StreamId StreamId { get; set; }

        [Id(1)]
        public byte[] Data { get; set; }
    }
}