// <copyright file="GrainsOptions.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Streaming.Grains.Streams
{
    public class GrainsOptions
    {
        public bool FireAndForgetDelivery { get; set; } = true;

        public TimeSpan RetryTimeout { get; set; } = TimeSpan.FromMinutes(1);

        public TimeSpan PoisonTimeout { get; internal set; } = TimeSpan.FromMinutes(3);
    }
}