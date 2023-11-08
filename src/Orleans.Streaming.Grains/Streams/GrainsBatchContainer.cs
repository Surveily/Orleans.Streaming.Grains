// <copyright file="GrainsBatchContainer.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Orleans.Providers;
using Orleans.Providers.Streams.Common;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Streams
{
    [Serializable]
    [GenerateSerializer]
    [SerializationCallbacks(typeof(OnDeserializedCallbacks))]
    internal sealed class GrainsBatchContainer<TSerializer> : IBatchContainer, IOnDeserialized
        where TSerializer : class, IMemoryMessageBodySerializer
    {
        [Id(0)]
        private readonly EventSequenceToken _realToken;

        [NonSerialized]
        private TSerializer _serializer;

        [NonSerialized]
        private MemoryMessageBody _payload;

        public GrainsBatchContainer(MemoryMessageData messageData, TSerializer serializer)
        {
            _serializer = serializer;
            MessageData = messageData;
            _realToken = new EventSequenceToken(messageData.SequenceNumber);
        }

        public StreamId StreamId => MessageData.StreamId;

        [Id(1)]
        public MemoryMessageData MessageData { get; set; }

        public StreamSequenceToken SequenceToken => _realToken;

        public long SequenceNumber => _realToken.SequenceNumber;

        public IEnumerable<Tuple<T, StreamSequenceToken>> GetEvents<T>()
        {
            return Payload().Events.Cast<T>().Select((e, i) => Tuple.Create<T, StreamSequenceToken>(e, _realToken.CreateSequenceTokenForEvent(i)));
        }

        public bool ImportRequestContext()
        {
            var context = Payload().RequestContext;

            if (context != null)
            {
                RequestContextExtensions.Import(context);
                return true;
            }

            return false;
        }

        void IOnDeserialized.OnDeserialized(DeserializationContext context)
        {
            _serializer = GrainsMessageBodySerializerFactory<TSerializer>.GetOrCreateSerializer(context.ServiceProvider);
        }

        private MemoryMessageBody Payload()
        {
            return _payload ?? (_payload = _serializer.Deserialize(MessageData.Payload));
        }
    }
}
