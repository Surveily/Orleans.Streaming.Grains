// <copyright file="GrainsMessageBodySerializer.cs" company="Surveily Sp. z o.o.">
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
    /// Default <see cref="IGrainsMessageBodySerializer"/> implementation.
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [SerializationCallbacks(typeof(Runtime.OnDeserializedCallbacks))]
    public sealed class GrainsMessageBodySerializer : IGrainsMessageBodySerializer, IOnDeserialized
    {
        [NonSerialized]
        private Serializer<GrainsMessageBody> _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrainsMessageBodySerializer" /> class.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        public GrainsMessageBodySerializer(Serializer<GrainsMessageBody> serializer)
        {
            _serializer = serializer;
        }

        /// <inheritdoc />
        public ArraySegment<byte> Serialize(GrainsMessageBody body)
        {
            return new ArraySegment<byte>(_serializer.SerializeToArray(body));
        }

        /// <inheritdoc />
        public GrainsMessageBody Deserialize(ArraySegment<byte> bodyBytes)
        {
            return _serializer.Deserialize(bodyBytes.ToArray());
        }

        /// <inheritdoc />
        void IOnDeserialized.OnDeserialized(DeserializationContext context)
        {
            _serializer = context.ServiceProvider.GetRequiredService<Serializer<GrainsMessageBody>>();
        }
    }
}
