// <copyright file="IClusterClientGrainsStreamConfigurator.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration;
using Orleans.Providers;

namespace Orleans.Streaming.Grains.Streams
{
    /// <summary>
    /// Client-specific configuration builder for memory streams.
    /// </summary>
    public interface IClusterClientGrainsStreamConfigurator : IGrainsStreamConfigurator, IClusterClientPersistentStreamConfigurator
    {
    }
}
