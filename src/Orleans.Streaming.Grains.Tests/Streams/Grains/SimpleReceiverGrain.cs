// <copyright file="SimpleReceiverGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Orleans.BroadcastChannel;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streaming.Grains.Tests.Streams.Messages;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Tests.Streams.Grains
{
    [ImplicitStreamSubscription(nameof(SimpleMessage))]
    [ImplicitStreamSubscription(nameof(BroadcastMessage))]
    [ImplicitChannelSubscription(nameof(SimpleMessage))]
    [ImplicitChannelSubscription(nameof(BroadcastMessage))]
    public class SimpleReceiverGrain : Grain, ISimpleReceiverGrain, IOnBroadcastChannelSubscribed
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
            var id = this.GetPrimaryKey();
            var streamProvider = ServiceProvider.GetKeyedService<IStreamProvider>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);

            if (streamProvider != null)
            {
                var stream = StreamFactory.Create<SimpleMessage>(streamProvider, id);
                var broadcastStream = StreamFactory.Create<BroadcastMessage>(streamProvider, id);

                _subscription = await stream.SubscribeAsync(OnNextAsync);
                _broadcast = await broadcastStream.SubscribeAsync(OnBroadcastAsync);
            }
        }

        public async Task OnSubscribed(IBroadcastChannelSubscription subscription)
        {
            await subscription.Attach<SimpleMessage>(OnNextAsync);
            await subscription.Attach<BroadcastMessage>(OnBroadcastAsync);
        }

        private async Task OnNextAsync(SimpleMessage message)
        {
            await OnNextAsync(message, null);
        }

        private Task OnNextAsync(SimpleMessage message, StreamSequenceToken token)
        {
            _processor.Process(message.Text.Value);

            return Task.CompletedTask;
        }

        private async Task OnBroadcastAsync(BroadcastMessage message)
        {
            await OnBroadcastAsync(message, null);
        }

        private Task OnBroadcastAsync(BroadcastMessage message, StreamSequenceToken token)
        {
            _processor.Process(message.Text.Value);

            return Task.CompletedTask;
        }
    }
}