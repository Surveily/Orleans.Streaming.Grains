// <copyright file="GrainsMessageBody.cs" company="Surveily Sp. z o.o.">
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
    /// Message body used by the in-memory stream provider.
    /// </summary>
    [Serializable]
    [GenerateSerializer]
    public sealed class GrainsMessageBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GrainsMessageBody"/> class.
        /// </summary>
        /// <param name="events">Events that are part of this message.</param>
        /// <param name="requestContext">Context in which this message was sent.</param>
        public GrainsMessageBody(IEnumerable<object> events, Dictionary<string, object> requestContext)
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            Events = events.ToList();
            RequestContext = requestContext;
        }

        /// <summary>
        /// Gets the events in the message.
        /// </summary>
        [Id(0)]
        public List<object> Events { get; }

        /// <summary>
        /// Gets the message request context.
        /// </summary>
        [Id(1)]
        public Dictionary<string, object> RequestContext { get; }
    }
}
