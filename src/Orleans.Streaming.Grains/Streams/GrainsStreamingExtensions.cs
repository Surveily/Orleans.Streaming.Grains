// <copyright file="GrainsStreamingExtensions.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Orleans.Providers;

namespace Orleans.Streaming.Grains.Streams
{
    public static class GrainsStreamingExtensions
    {
        /// <summary>
        /// Adds a new in-memory stream provider to the client, using the default message serializer
        /// (<see cref="DefaultMemoryMessageBodySerializer"/>).
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="name">The stream provider name.</param>
        /// <param name="configure">The configuration delegate.</param>
        /// <returns>The client builder.</returns>
        public static IClientBuilder AddMemoryStreams2(
            this IClientBuilder builder,
            string name,
            Action<IClusterClientGrainsStreamConfigurator> configure = null)
        {
            return AddMemoryStreams2<DefaultMemoryMessageBodySerializer>(builder, name, configure);
        }

        /// <summary>
        /// Adds a new in-memory stream provider to the client.
        /// </summary>
        /// <typeparam name="TSerializer">The type of the t serializer.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="name">The stream provider name.</param>
        /// <param name="configure">The configuration delegate.</param>
        /// <returns>The client builder.</returns>
        public static IClientBuilder AddMemoryStreams2<TSerializer>(
            this IClientBuilder builder,
            string name,
            Action<IClusterClientGrainsStreamConfigurator> configure = null)
            where TSerializer : class, IMemoryMessageBodySerializer
        {
            var memoryStreamConfigurator = new GrainsStreamConfigurator<TSerializer>(name, builder);
            configure?.Invoke(memoryStreamConfigurator);
            return builder;
        }

        /// <summary>
        /// Configure silo to use memory streams, using the default message serializer
        /// (<see cref="DefaultMemoryMessageBodySerializer"/>).
        /// </summary>
        /// using the default built-in serializer
        /// <param name="builder">The builder.</param>
        /// <param name="name">The stream provider name.</param>
        /// <param name="configure">The configuration delegate.</param>
        /// <returns>The silo builder.</returns>
        public static ISiloBuilder AddMemoryStreams2(this ISiloBuilder builder, string name,
                Action<ISiloMemoryStreamConfigurator> configure = null)
        {
            return AddMemoryStreams2<DefaultMemoryMessageBodySerializer>(builder, name, configure);
        }

        /// <summary>
        /// Configure silo to use memory streams.
        /// </summary>
        /// <typeparam name="TSerializer">The message serializer type, which must implement <see cref="IMemoryMessageBodySerializer"/>.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="name">The stream provider name.</param>
        /// <param name="configure">The configuration delegate.</param>
        /// <returns>The silo builder.</returns>
        public static ISiloBuilder AddMemoryStreams2<TSerializer>(this ISiloBuilder builder, string name,
            Action<ISiloMemoryStreamConfigurator> configure = null)
             where TSerializer : class, IMemoryMessageBodySerializer
        {
            var memoryStreamConfiguretor = new SiloGrainsStreamConfigurator<TSerializer>(name, configureDelegate => builder.ConfigureServices(configureDelegate));

            configure?.Invoke(memoryStreamConfiguretor);
            return builder;
        }
    }
}