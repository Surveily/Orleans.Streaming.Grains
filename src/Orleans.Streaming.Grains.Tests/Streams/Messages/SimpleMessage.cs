// <copyright file="SimpleMessage.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Orleans.Concurrency;

namespace Orleans.Streaming.Grains.Tests.Streams.Messages
{
    [GenerateSerializer]
    public class SimpleMessage
    {
        [Id(0)]
        public Immutable<string> Text { get; set; }
    }
}