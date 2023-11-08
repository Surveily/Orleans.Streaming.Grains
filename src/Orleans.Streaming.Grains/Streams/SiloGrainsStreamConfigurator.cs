// <copyright file="SiloGrainsStreamConfigurator.cs" company="Surveily Sp. z o.o.">
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
    /// <typeparam name="TSerializer">The message body serializer type, which must implement <see cref="IMemoryMessageBodySerializer"/>.</typeparam>
    public class SiloGrainsStreamConfigurator<TSerializer> : SiloRecoverableStreamConfigurator, ISiloMemoryStreamConfigurator
          where TSerializer : class, IMemoryMessageBodySerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiloGrainsStreamConfigurator{TSerializer}"/> class.
        /// </summary>
        /// <param name="name">The stream provider name.</param>
        /// <param name="configureServicesDelegate">The services configuration delegate.</param>
        public SiloGrainsStreamConfigurator(
            string name, Action<Action<IServiceCollection>> configureServicesDelegate)
            : base(name, configureServicesDelegate, GrainsAdapterFactory<TSerializer>.Create)
        {
            ConfigureDelegate(services => services.ConfigureNamedOptionForLogging<HashRingStreamQueueMapperOptions>(name));
        }
    }
}
