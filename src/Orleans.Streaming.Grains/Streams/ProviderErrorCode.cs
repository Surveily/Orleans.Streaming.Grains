// <copyright file="ProviderErrorCode.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

namespace Orleans.Streaming.Grains.Streams
{
    public enum ProviderErrorCode
    {
        /// <summary>
        /// 200000
        /// </summary>
        ProvidersBase = 200000,

        /// <summary>
        /// 200400
        /// </summary>
        MemoryStreamProviderBase = ProvidersBase + 400,

        /// <summary>
        /// 200401
        /// </summary>
        MemoryStreamProviderBase_QueueMessageBatchAsync = MemoryStreamProviderBase + 1,

        /// <summary>
        /// 200402
        /// </summary>
        MemoryStreamProviderBase_GetQueueMessagesAsync = MemoryStreamProviderBase + 2,
    }
}