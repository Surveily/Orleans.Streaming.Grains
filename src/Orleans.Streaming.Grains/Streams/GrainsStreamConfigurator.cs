// <copyright file="GrainsStreamConfigurator.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration;
using Orleans.Providers;

namespace Orleans.Streaming.Grains.Streams
{
    /// <summary>
    /// Configures memory streams.
    /// </summary>
    /// <typeparam name="TSerializer">The message body serializer type, which must implement <see cref="IGrainsMessageBodySerializer"/>.</typeparam>
    public class GrainsStreamConfigurator<TSerializer> : ClusterClientPersistentStreamConfigurator, IClusterClientGrainsStreamConfigurator
          where TSerializer : class, IGrainsMessageBodySerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GrainsStreamConfigurator{TSerializer}"/> class.
        /// </summary>
        /// <param name="name">The stream provider name.</param>
        /// <param name="builder">The builder.</param>
        public GrainsStreamConfigurator(string name, IClientBuilder builder)
         : base(name, builder, GrainsAdapterFactory<TSerializer>.Create)
        {
        }
    }
}
