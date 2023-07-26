// <copyright file="BlobReceiverGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Orleans.Runtime;
using Orleans.Streaming.Grains.Tests.Streams.Messages;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Tests.Streams.Grains
{
    [ImplicitStreamSubscription(nameof(BlobMessage))]
    [ImplicitStreamSubscription(nameof(BroadcastMessage))]
    public class BlobReceiverGrain : Grain, IBlobReceiverGrain
    {
        private readonly IProcessor _processor;

        private StreamSubscriptionHandle<BlobMessage> _subscription;
        private StreamSubscriptionHandle<BroadcastMessage> _broadcast;

        public BlobReceiverGrain(IProcessor processor)
        {
            _processor = processor;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider("Default");
            var stream = StreamFactory.Create<BlobMessage>(streamProvider, this.GetPrimaryKey());
            var broadcastStream = StreamFactory.Create<BroadcastMessage>(streamProvider, this.GetPrimaryKey());

            _subscription = await stream.SubscribeAsync(OnNextAsync);
            _broadcast = await broadcastStream.SubscribeAsync(OnBroadcastAsync);
        }

        private Task OnNextAsync(BlobMessage message, StreamSequenceToken token)
        {
            _processor.Process(message.Data.Value);

            return Task.CompletedTask;
        }

        private Task OnBroadcastAsync(BroadcastMessage message, StreamSequenceToken token)
        {
            _processor.Process(message.Data.Value);

            return Task.CompletedTask;
        }
    }
}