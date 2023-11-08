// <copyright file="IGrainsStreamConfigurator.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration;
using Orleans.Providers;

namespace Orleans.Streaming.Grains.Streams
{
    /// <summary>
    /// Configuration builder for memory streams.
    /// </summary>
    public interface IGrainsStreamConfigurator : INamedServiceConfigurator
    {
    }
}
