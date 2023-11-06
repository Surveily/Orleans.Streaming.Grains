// <copyright file="IGrainsMessageBodySerializer.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Serialization;

namespace Orleans.Streaming.Grains.Streams
{
    /// <summary>
    /// Implementations of this interface are responsible for serializing MemoryMessageBody objects.
    /// </summary>
    public interface IGrainsMessageBodySerializer
    {
        /// <summary>
        /// Serialize <see cref="GrainsMessageBody"/> to an array segment of bytes.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns>The serialized data.</returns>
        ArraySegment<byte> Serialize(GrainsMessageBody body);

        /// <summary>
        /// Deserialize an array segment into a <see cref="GrainsMessageBody"/>.
        /// </summary>
        /// <param name="bodyBytes">The body bytes.</param>
        /// <returns>The deserialized message body.</returns>
        GrainsMessageBody Deserialize(ArraySegment<byte> bodyBytes);
    }
}
