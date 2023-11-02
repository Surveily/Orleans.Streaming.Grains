// <copyright file="GrainsBatchContainer.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class GrainsBatchContainer : IBatchContainer, IOnDeserialized
    {
        /// <summary>
        /// Need to store reference to the original Message to be able to delete it later on.
        /// </summary>
        [NonSerialized]
        public Guid Id;

        private static readonly Lazy<ObjectFactory> ObjectFactory = new Lazy<ObjectFactory>(() => ActivatorUtilities.CreateFactory(typeof(DefaultMemoryMessageBodySerializer), Type.EmptyTypes));

        [Id(0)]
        private readonly EventSequenceToken _sequenceToken;

        [NonSerialized]
        private MemoryMessageBody _payload;

        [NonSerialized]
        private IMemoryMessageBodySerializer _serializer;

        public GrainsBatchContainer(MemoryMessageData messageData, IMemoryMessageBodySerializer serializer)
        {
            _serializer = serializer;
            _sequenceToken = new EventSequenceToken(messageData.SequenceNumber);
        }

        [Id(1)]
        public MemoryMessageData MessageData { get; set; }

        public StreamId StreamId => MessageData.StreamId;

        public StreamSequenceToken SequenceToken => _sequenceToken;

        public IEnumerable<Tuple<T, StreamSequenceToken>> GetEvents<T>()
        {
            return Payload().Events
                            .Cast<T>()
                            .Select((e, i) => Tuple.Create<T, StreamSequenceToken>(e, _sequenceToken.CreateSequenceTokenForEvent(i)));
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

        public override string ToString()
        {
            return string.Format($"[{nameof(GrainsBatchContainer)}:Stream={0},#Items={1}]", StreamId, Payload().Events.Count);
        }

        public void OnDeserialized(DeserializationContext context)
        {
            _serializer = context.ServiceProvider.GetService<DefaultMemoryMessageBodySerializer>() ?? ((DefaultMemoryMessageBodySerializer)ObjectFactory.Value(context.ServiceProvider, null));
        }

        private MemoryMessageBody Payload()
        {
            return _payload ?? (_payload = _serializer.Deserialize(MessageData.Payload));
        }
    }
}