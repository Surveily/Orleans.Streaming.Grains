// <copyright file="IEmitterGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

namespace Orleans.Streaming.Grains.Tests.Streams.Grains
{
    public interface IEmitterGrain : IGrainWithGuidKey
    {
        Task SendAsync(string text);

        Task SendAsync(byte[] data);

        Task SendAsync(string text, byte[] data);

        Task ExplosiveAsync(string text, byte[] data);

        Task BroadcastAsync(string text, byte[] data);
    }
}