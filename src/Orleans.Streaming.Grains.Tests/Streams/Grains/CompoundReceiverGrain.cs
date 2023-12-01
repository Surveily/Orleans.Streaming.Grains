// <copyright file="CompoundReceiverGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.BroadcastChannel;
using Orleans.Concurrency;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streaming.Grains.Tests.Streams.Messages;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Tests.Streams.Grains
{
    [ImplicitStreamSubscription(nameof(CompoundMessage))]
    [ImplicitChannelSubscription(nameof(CompoundMessage))]
    public class CompoundReceiverGrain : Grain, ICompoundReceiverGrain, IOnBroadcastChannelSubscribed
    {
        private IAsyncStream<BlobMessage> _blobStream;
        private IAsyncStream<SimpleMessage> _simpleStream;
        private IBroadcastChannelWriter<BlobMessage> _blobChannel;
        private IBroadcastChannelWriter<SimpleMessage> _simpleChannel;
        private StreamSubscriptionHandle<CompoundMessage> _subscription;

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var id = this.GetPrimaryKey();
            var streamProvider = ServiceProvider.GetServiceByName<IStreamProvider>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);
            var broadcastProvider = ServiceProvider.GetServiceByName<IBroadcastChannelProvider>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);

            if (streamProvider != null)
            {
                var stream = StreamFactory.Create<CompoundMessage>(streamProvider, id);

                _subscription = await stream.SubscribeAsync(OnNextAsync);
                _blobStream = StreamFactory.Create<BlobMessage>(streamProvider, id);
                _simpleStream = StreamFactory.Create<SimpleMessage>(streamProvider, id);
            }

            if (broadcastProvider != null)
            {
                _blobChannel = broadcastProvider.GetChannelWriter<BlobMessage>(ChannelId.Create(nameof(BlobMessage), id));
                _simpleChannel = broadcastProvider.GetChannelWriter<SimpleMessage>(ChannelId.Create(nameof(SimpleMessage), id));
            }

            await base.OnActivateAsync(cancellationToken);
        }

        public async Task OnSubscribed(IBroadcastChannelSubscription subscription)
        {
            await subscription.Attach<CompoundMessage>(OnNextAsync);
        }

        private async Task OnNextAsync(CompoundMessage message)
        {
            await _blobChannel.Publish(new BlobMessage
            {
                Data = message.Data,
            });

            await _simpleChannel.Publish(new SimpleMessage
            {
                Text = message.Text,
            });
        }

        private async Task OnNextAsync(CompoundMessage message, StreamSequenceToken token)
        {
            await _blobStream.OnNextAsync(new BlobMessage
            {
                Data = message.Data,
            });

            await _simpleStream.OnNextAsync(new SimpleMessage
            {
                Text = message.Text,
            });
        }
    }
}