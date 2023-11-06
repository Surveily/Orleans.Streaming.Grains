// <copyright file="GrainsMessageBodySerializerFactory.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Orleans.Streaming.Grains.Streams
{
    internal static class GrainsMessageBodySerializerFactory<TSerializer>
        where TSerializer : class, IGrainsMessageBodySerializer
    {
        private static readonly Lazy<ObjectFactory> ObjectFactory = new Lazy<ObjectFactory>(
            () => ActivatorUtilities.CreateFactory(
                typeof(TSerializer),
                Type.EmptyTypes));

        public static TSerializer GetOrCreateSerializer(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<TSerializer>() ??
                   (TSerializer)ObjectFactory.Value(serviceProvider, null);
        }
    }
}