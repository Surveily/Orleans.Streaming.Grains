// <copyright file="CompoundReceiverGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.Concurrency;
using Orleans.Streaming.Grains.Tests.Streams.Messages;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Tests.Streams.Grains
{
    [ImplicitStreamSubscription(nameof(CompoundMessage))]
    public class CompoundReceiverGrain : Grain, ICompoundReceiverGrain
    {
        private IAsyncStream<BlobMessage> _blobStream;
        private IAsyncStream<SimpleMessage> _simpleStream;
        private StreamSubscriptionHandle<CompoundMessage> _subscription;

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider("Default");
            var stream = StreamFactory.Create<CompoundMessage>(streamProvider, this.GetPrimaryKey());

            _subscription = await stream.SubscribeAsync(OnNextAsync);
            _blobStream = StreamFactory.Create<BlobMessage>(streamProvider, this.GetPrimaryKey());
            _simpleStream = StreamFactory.Create<SimpleMessage>(streamProvider, this.GetPrimaryKey());

            await base.OnActivateAsync(cancellationToken);
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