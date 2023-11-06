// <copyright file="GrainsStreamConfiguratorExtensions.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration;
using Orleans.Providers;

namespace Orleans.Streaming.Grains.Streams
{
    /// <summary>
    /// Configuration extensions for memory streams.
    /// </summary>
    public static class GrainsStreamConfiguratorExtensions
    {
        /// <summary>
        /// Configures partitioning for memory streams.
        /// </summary>
        /// <param name="configurator">The configuration builder.</param>
        /// <param name="numOfQueues">The number of queues.</param>
        public static void ConfigurePartitioning(this IGrainsStreamConfigurator configurator, int numOfQueues = HashRingStreamQueueMapperOptions.DEFAULT_NUM_QUEUES)
        {
            configurator.Configure<HashRingStreamQueueMapperOptions>(ob => ob.Configure(options => options.TotalQueueCount = numOfQueues));
        }
    }
}
