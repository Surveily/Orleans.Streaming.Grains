// <copyright file="EmitterGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Orleans.Concurrency;
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

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var id = this.GetPrimaryKey();
            var streamProvider = this.GetStreamProvider("Default");

            _blobStream = StreamFactory.Create<BlobMessage>(streamProvider, id);
            _simpleStream = StreamFactory.Create<SimpleMessage>(streamProvider, id);
            _compoundStream = StreamFactory.Create<CompoundMessage>(streamProvider, id);
            _broadcastStream = StreamFactory.Create<BroadcastMessage>(streamProvider, id);
            _explosiveStream = StreamFactory.Create<ExplosiveMessage>(streamProvider, id);

            await base.OnActivateAsync(cancellationToken);
        }

        public async Task SendAsync(string text)
        {
            await _simpleStream.OnNextAsync(new SimpleMessage
            {
                Text = new Immutable<string>(text),
            });
        }

        public async Task SendAsync(byte[] data)
        {
            await _blobStream.OnNextAsync(new BlobMessage
            {
                Data = new Immutable<byte[]>(data),
            });
        }

        public async Task CompoundAsync(string text, byte[] data)
        {
            await _compoundStream.OnNextAsync(new CompoundMessage
            {
                Text = new Immutable<string>(text),
                Data = new Immutable<byte[]>(data),
            });
        }

        public async Task ExplosiveAsync(string text, byte[] data)
        {
            await _explosiveStream.OnNextAsync(new ExplosiveMessage
            {
                Text = new Immutable<string>(text),
                Data = new Immutable<byte[]>(data),
            });
        }

        public async Task BroadcastAsync(string text, byte[] data)
        {
            await _broadcastStream.OnNextAsync(new BroadcastMessage
            {
                Text = new Immutable<string>(text),
                Data = new Immutable<byte[]>(data),
            });
        }
    }
}