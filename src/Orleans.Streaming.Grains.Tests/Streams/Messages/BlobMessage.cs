// <copyright file="BlobMessage.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Orleans.Concurrency;

namespace Orleans.Streaming.Grains.Tests.Streams.Messages
{
    [GenerateSerializer]
    public class BlobMessage
    {
        [Id(0)]
        public Immutable<byte[]> Data { get; set; }
    }
}