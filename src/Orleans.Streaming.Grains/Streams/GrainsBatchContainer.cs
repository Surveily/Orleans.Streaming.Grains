// <copyright file="GrainsBatchContainer.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orleans.Providers.Streams.Common;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Streams
{
    /// <summary>
    /// Generic container for Grains events.
    /// </summary>
    [Serializable]
    [GenerateSerializer]
    public class GrainsBatchContainer : IBatchContainer
    {
        /// <summary>
        /// Need to store reference to the original Message to be able to delete it later on.
        /// </summary>
        [NonSerialized]
        public Guid Id;

        [JsonProperty]
        [Id(1)]
        private readonly List<object> _events;

        [JsonProperty]
        [Id(2)]
        private readonly Dictionary<string, object> _requestContext;

        [JsonProperty]
        [Id(0)]
        private EventSequenceTokenV2 _sequenceToken;

        private GrainsBatchContainer(StreamId streamId,
                                     List<object> events,
                                     Dictionary<string, object> requestContext)
        {
            StreamId = streamId;

            _requestContext = requestContext;
            _events = events ?? throw new ArgumentNullException(nameof(events), "Message contains no events");
        }

        [Id(3)]
        public StreamId StreamId { get; }

        public StreamSequenceToken SequenceToken => _sequenceToken;

        public IEnumerable<Tuple<T, StreamSequenceToken>> GetEvents<T>()
        {
            return _events.OfType<T>().Select((e, i) => Tuple.Create<T, StreamSequenceToken>(e, _sequenceToken.CreateSequenceTokenForEvent(i)));
        }

        public bool ImportRequestContext()
        {
            if (_requestContext != null)
            {
                RequestContextExtensions.Import(_requestContext);
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format($"[{nameof(GrainsBatchContainer)}:Stream={0},#Items={1}]", StreamId, _events.Count);
        }

        internal static GrainsMessage ToMessage<T>(Serializer<GrainsBatchContainer> serializer, StreamId streamId, IEnumerable<T> events, Dictionary<string, object> requestContext)
        {
            var batchMessage = new GrainsBatchContainer(streamId, events.Cast<object>().ToList(), requestContext);
            var rawBytes = serializer.SerializeToArray(batchMessage);

            return new GrainsMessage
            {
                StreamId = streamId,
                Data = rawBytes
            };
        }

        internal static GrainsBatchContainer FromMessage(Serializer<GrainsBatchContainer> serializer, Guid id, GrainsMessage msg, long sequenceId)
        {
            if (msg != null)
            {
                var batch = serializer.Deserialize(msg.Data);

                batch.Id = id;
                batch._sequenceToken = new EventSequenceTokenV2(sequenceId);

                return batch;
            }

            throw new InvalidOperationException("Payload is null");
        }
    }
}