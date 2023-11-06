// <copyright file="IGrainsStreamQueueGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Streaming.Grains.Streams
{
    /// <summary>
    /// Interface for In-memory stream queue grain.
    /// </summary>
    public interface IGrainsStreamQueueGrain : IGrainWithGuidKey
    {
        /// <summary>
        /// Enqueues an event.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>A <see cref="Task"/> representing the operation.</returns>
        Task Enqueue(GrainsMessageData data);

        /// <summary>
        /// Dequeues up to <paramref name="maxCount"/> events.
        /// </summary>
        /// <param name="maxCount">
        /// The maximum number of events to dequeue.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the operation.</returns>
        Task<List<GrainsMessageData>> Dequeue(int maxCount);
    }
}
