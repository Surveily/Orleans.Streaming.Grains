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

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var id = this.GetPrimaryKey();
            var streamProvider = this.GetStreamProvider("Default");

            _blobStream = StreamFactory.Create<BlobMessage>(streamProvider, id);
            _simpleStream = StreamFactory.Create<SimpleMessage>(streamProvider, id);
            _compoundStream = StreamFactory.Create<CompoundMessage>(streamProvider, id);

            await base.OnActivateAsync(cancellationToken);
        }

        public async Task SendAsync(string text)
        {
            if (_simpleStream != null)
            {
                await _simpleStream.OnNextAsync(new SimpleMessage
                {
                    Text = new Immutable<string>(text),
                });
            }
        }

        public async Task SendAsync(byte[] data)
        {
            if (_blobStream != null)
            {
                await _blobStream.OnNextAsync(new BlobMessage
                {
                    Data = new Immutable<byte[]>(data),
                });
            }
        }

        public async Task SendAsync(string text, byte[] data)
        {
            if (_compoundStream != null)
            {
                await _compoundStream.OnNextAsync(new CompoundMessage
                {
                    Text = new Immutable<string>(text),
                    Data = new Immutable<byte[]>(data),
                });
            }
        }
    }
}