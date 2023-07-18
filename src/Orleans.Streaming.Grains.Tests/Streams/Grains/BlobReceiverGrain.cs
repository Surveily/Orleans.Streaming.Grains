// <copyright file="BlobReceiverGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Orleans.Runtime;
using Orleans.Streaming.Grains.Tests.Streams.Messages;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Tests.Streams.Grains
{
    [ImplicitStreamSubscription(nameof(BlobMessage))]
    [ImplicitStreamSubscription(nameof(CompoundMessage))]
    public class BlobReceiverGrain : Grain, IBlobReceiverGrain
    {
        private readonly IProcessor _processor;

        private StreamSubscriptionHandle<BlobMessage> _blobSubscription;
        private StreamSubscriptionHandle<CompoundMessage> _compoundSubscription;

        public BlobReceiverGrain(IProcessor processor)
        {
            _processor = processor;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider("Default");
            var blobStream = StreamFactory.Create<BlobMessage>(streamProvider, this.GetPrimaryKey());
            var compoundStream = StreamFactory.Create<CompoundMessage>(streamProvider, this.GetPrimaryKey());

            _blobSubscription = await blobStream.SubscribeAsync(OnNextBlobAsync);
            _compoundSubscription = await compoundStream.SubscribeAsync(OnNextCompoundAsync);
        }

        private Task OnNextBlobAsync(BlobMessage message, StreamSequenceToken token)
        {
            _processor.Process(message.Data.Value);

            return Task.CompletedTask;
        }

        private Task OnNextCompoundAsync(CompoundMessage message, StreamSequenceToken token)
        {
            _processor.Process(message.Data.Value);

            return Task.CompletedTask;
        }
    }
}