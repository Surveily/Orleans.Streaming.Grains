// <copyright file="IProcessor.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

namespace Orleans.Streaming.Grains.Tests.Streams.Messages
{
    public interface IProcessor
    {
        void Process(string text);

        void Process(byte[] data);
    }
}