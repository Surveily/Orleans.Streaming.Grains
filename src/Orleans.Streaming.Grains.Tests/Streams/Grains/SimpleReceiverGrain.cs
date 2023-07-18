// <copyright file="SimpleReceiverGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Orleans.Runtime;
using Orleans.Streaming.Grains.Tests.Streams.Messages;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Tests.Streams.Grains
{
    [ImplicitStreamSubscription(nameof(SimpleMessage))]
    [ImplicitStreamSubscription(nameof(CompoundMessage))]
    public class SimpleReceiverGrain : Grain, ISimpleReceiverGrain
    {
        private readonly IProcessor _processor;

        private StreamSubscriptionHandle<SimpleMessage> _textSubscription;
        private StreamSubscriptionHandle<CompoundMessage> _compoundSubscription;

        public SimpleReceiverGrain(IProcessor processor)
        {
            _processor = processor;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider("Default");
            var textStream = StreamFactory.Create<SimpleMessage>(streamProvider, this.GetPrimaryKey());
            var compoundStream = StreamFactory.Create<CompoundMessage>(streamProvider, this.GetPrimaryKey());

            _textSubscription = await textStream.SubscribeAsync(OnNextAsync);
            _compoundSubscription = await compoundStream.SubscribeAsync(OnNextCompoundAsync);
        }

        private Task OnNextAsync(SimpleMessage message, StreamSequenceToken token)
        {
            _processor.Process(message.Text.Value);

            return Task.CompletedTask;
        }

        private Task OnNextCompoundAsync(CompoundMessage message, StreamSequenceToken token)
        {
            _processor.Process(message.Text.Value);

            return Task.CompletedTask;
        }
    }
}