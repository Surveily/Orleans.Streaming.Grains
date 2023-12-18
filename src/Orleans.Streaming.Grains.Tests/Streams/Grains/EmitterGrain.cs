// <copyright file="EmitterGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Orleans.BroadcastChannel;
using Orleans.Concurrency;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streaming.Grains.Tests.Streams.Messages;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Tests.Streams.Grains
{
    public class EmitterGrain : Grain, IEmitterGrain
    {
        private IAsyncStream<BlobMessage> _blobStream;
        private IAsyncStream<SimpleMessage> _simpleStream;
        private IAsyncStream<CompoundMessage> _compoundStream;
        private IAsyncStream<ExplosiveMessage> _explosiveStream;
        private IAsyncStream<BroadcastMessage> _broadcastStream;
        private IBroadcastChannelWriter<BlobMessage> _blobChannel;
        private IBroadcastChannelWriter<SimpleMessage> _simpleChannel;
        private IBroadcastChannelWriter<CompoundMessage> _compoundChannel;
        private IBroadcastChannelWriter<BroadcastMessage> _broadcastChannel;
        private IBroadcastChannelWriter<ExplosiveMessage> _explosiveChannel;

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var id = this.GetPrimaryKey();
            var streamProvider = ServiceProvider.GetKeyedService<IStreamProvider>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);
            var broadcastProvider = ServiceProvider.GetKeyedService<IBroadcastChannelProvider>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);

            if (streamProvider != null)
            {
                _blobStream = StreamFactory.Create<BlobMessage>(streamProvider, id);
                _simpleStream = StreamFactory.Create<SimpleMessage>(streamProvider, id);
                _compoundStream = StreamFactory.Create<CompoundMessage>(streamProvider, id);
                _broadcastStream = StreamFactory.Create<BroadcastMessage>(streamProvider, id);
                _explosiveStream = StreamFactory.Create<ExplosiveMessage>(streamProvider, id);
            }

            if (broadcastProvider != null)
            {
                _blobChannel = broadcastProvider.GetChannelWriter<BlobMessage>(ChannelId.Create(nameof(BlobMessage), id));
                _simpleChannel = broadcastProvider.GetChannelWriter<SimpleMessage>(ChannelId.Create(nameof(SimpleMessage), id));
                _compoundChannel = broadcastProvider.GetChannelWriter<CompoundMessage>(ChannelId.Create(nameof(CompoundMessage), id));
                _broadcastChannel = broadcastProvider.GetChannelWriter<BroadcastMessage>(ChannelId.Create(nameof(BroadcastMessage), id));
                _explosiveChannel = broadcastProvider.GetChannelWriter<ExplosiveMessage>(ChannelId.Create(nameof(ExplosiveMessage), id));
            }

            await base.OnActivateAsync(cancellationToken);
        }

        public async Task SendAsync(string text)
        {
            var item = new SimpleMessage
            {
                Text = new Immutable<string>(text),
            };

            await (_simpleChannel?.Publish(item) ?? Task.CompletedTask);
            await (_simpleStream?.OnNextAsync(item) ?? Task.CompletedTask);
        }

        public async Task SendAsync(byte[] data)
        {
            var item = new BlobMessage
            {
                Data = new Immutable<byte[]>(data),
            };

            await (_blobChannel?.Publish(item) ?? Task.CompletedTask);
            await (_blobStream?.OnNextAsync(item) ?? Task.CompletedTask);
        }

        public async Task CompoundAsync(string text, byte[] data)
        {
            var item = new CompoundMessage
            {
                Text = new Immutable<string>(text),
                Data = new Immutable<byte[]>(data),
            };

            await (_compoundChannel?.Publish(item) ?? Task.CompletedTask);
            await (_compoundStream?.OnNextAsync(item) ?? Task.CompletedTask);
        }

        public async Task ExplosiveAsync(string text, byte[] data)
        {
            var item = new ExplosiveMessage
            {
                Text = new Immutable<string>(text),
                Data = new Immutable<byte[]>(data),
            };

            await (_explosiveChannel?.Publish(item) ?? Task.CompletedTask);
            await (_explosiveStream?.OnNextAsync(item) ?? Task.CompletedTask);
        }

        public async Task BroadcastAsync(string text, byte[] data)
        {
            var item = new BroadcastMessage
            {
                Text = new Immutable<string>(text),
                Data = new Immutable<byte[]>(data),
            };

            await (_broadcastChannel?.Publish(item) ?? Task.CompletedTask);
            await (_broadcastStream?.OnNextAsync(item) ?? Task.CompletedTask);
        }
    }
}