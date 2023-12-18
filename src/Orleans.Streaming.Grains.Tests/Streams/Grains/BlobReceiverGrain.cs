// <copyright file="BlobReceiverGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Orleans.BroadcastChannel;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streaming.Grains.Tests.Streams.Messages;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Tests.Streams.Grains
{
    [ImplicitStreamSubscription(nameof(BlobMessage))]
    [ImplicitStreamSubscription(nameof(BroadcastMessage))]
    [ImplicitChannelSubscription(nameof(BlobMessage))]
    [ImplicitChannelSubscription(nameof(BroadcastMessage))]
    public class BlobReceiverGrain : Grain, IBlobReceiverGrain, IOnBroadcastChannelSubscribed
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
            var id = this.GetPrimaryKey();
            var streamProvider = ServiceProvider.GetKeyedService<IStreamProvider>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);

            if (streamProvider != null)
            {
                var stream = StreamFactory.Create<BlobMessage>(streamProvider, id);
                var broadcastStream = StreamFactory.Create<BroadcastMessage>(streamProvider, id);

                _subscription = await stream.SubscribeAsync(OnNextAsync);
                _broadcast = await broadcastStream.SubscribeAsync(OnBroadcastAsync);
            }
        }

        public async Task OnSubscribed(IBroadcastChannelSubscription subscription)
        {
            var id = subscription.ChannelId;
            var message = Encoding.Default.GetString(id.Namespace.ToArray());

            // TODO: Channels don't support multiple subs.
            if (message == nameof(BlobMessage))
            {
                await subscription.Attach<BlobMessage>(OnNextAsync);
            }

            if (message == nameof(BroadcastMessage))
            {
                await subscription.Attach<BroadcastMessage>(OnBroadcastAsync);
            }
        }

        private async Task OnNextAsync(BlobMessage message)
        {
            await OnNextAsync(message, null);
        }

        private Task OnNextAsync(BlobMessage message, StreamSequenceToken token)
        {
            _processor.Process(message.Data.Value);

            return Task.CompletedTask;
        }

        private async Task OnBroadcastAsync(BroadcastMessage message)
        {
            await OnBroadcastAsync(message, null);
        }

        private Task OnBroadcastAsync(BroadcastMessage message, StreamSequenceToken token)
        {
            _processor.Process(message.Data.Value);

            return Task.CompletedTask;
        }
    }
}