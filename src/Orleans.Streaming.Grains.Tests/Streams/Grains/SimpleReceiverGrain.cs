// <copyright file="SimpleReceiverGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Orleans.Runtime;
using Orleans.Streaming.Grains.Tests.Streams.Messages;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Tests.Streams.Grains
{
    [ImplicitStreamSubscription(nameof(SimpleMessage))]
    [ImplicitStreamSubscription(nameof(BroadcastMessage))]
    public class SimpleReceiverGrain : Grain, ISimpleReceiverGrain
    {
        private readonly IProcessor _processor;

        private StreamSubscriptionHandle<SimpleMessage> _subscription;
        private StreamSubscriptionHandle<BroadcastMessage> _broadcast;

        public SimpleReceiverGrain(IProcessor processor)
        {
            _processor = processor;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider("Default");
            var stream = StreamFactory.Create<SimpleMessage>(streamProvider, this.GetPrimaryKey());
            var broadcastStream = StreamFactory.Create<BroadcastMessage>(streamProvider, this.GetPrimaryKey());

            _subscription = await stream.SubscribeAsync(OnNextAsync);
            _broadcast = await broadcastStream.SubscribeAsync(OnBroadcastAsync);
        }

        private Task OnNextAsync(SimpleMessage message, StreamSequenceToken token)
        {
            _processor.Process(message.Text.Value);

            return Task.CompletedTask;
        }

        private Task OnBroadcastAsync(BroadcastMessage message, StreamSequenceToken token)
        {
            _processor.Process(message.Text.Value);

            return Task.CompletedTask;
        }
    }
}