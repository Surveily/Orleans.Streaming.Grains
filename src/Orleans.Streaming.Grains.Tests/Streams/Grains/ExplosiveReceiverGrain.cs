// <copyright file="ExplosiveReceiverGrain.cs" company="Surveily Sp. z o.o.">
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
    [ImplicitStreamSubscription(nameof(ExplosiveMessage))]
    public class ExplosiveReceiverGrain : Grain, IExplosiveReceiverGrain
    {
        private IAsyncStream<ExplosiveNextMessage> _nextStream;
        private StreamSubscriptionHandle<ExplosiveMessage> _subscription;

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider("Default");
            var stream = StreamFactory.Create<ExplosiveMessage>(streamProvider, this.GetPrimaryKey());

            _subscription = await stream.SubscribeAsync(OnNextAsync);
            _nextStream = StreamFactory.Create<ExplosiveNextMessage>(streamProvider, this.GetPrimaryKey());

            await base.OnActivateAsync(cancellationToken);
        }

        private async Task OnNextAsync(ExplosiveMessage message, StreamSequenceToken token)
        {
            await _nextStream.OnNextAsync(new ExplosiveNextMessage
            {
                Data = message.Data,
                Text = message.Text,
            });
        }
    }

    [ImplicitStreamSubscription(nameof(ExplosiveNextMessage))]
    public class ExplosiveNextFirstReceiverGrain : Grain, IExplosiveReceiverGrain
    {
        private IAsyncStream<CompoundMessage> _compoundStream;
        private StreamSubscriptionHandle<ExplosiveNextMessage> _subscription;

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider("Default");
            var stream = StreamFactory.Create<ExplosiveNextMessage>(streamProvider, this.GetPrimaryKey());

            _subscription = await stream.SubscribeAsync(OnNextAsync);
            _compoundStream = StreamFactory.Create<CompoundMessage>(streamProvider, this.GetPrimaryKey());

            await base.OnActivateAsync(cancellationToken);
        }

        private async Task OnNextAsync(ExplosiveNextMessage message, StreamSequenceToken token)
        {
            for (var i = 0; i < 10; i++)
            {
                await _compoundStream.OnNextAsync(new CompoundMessage
                {
                    Data = message.Data,
                    Text = message.Text,
                });
            }
        }
    }

    [ImplicitStreamSubscription(nameof(ExplosiveNextMessage))]
    public class ExplosiveNextSecondReceiverGrain : Grain, IExplosiveReceiverGrain
    {
        private IAsyncStream<CompoundMessage> _compoundStream;
        private StreamSubscriptionHandle<ExplosiveNextMessage> _subscription;

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider("Default");
            var stream = StreamFactory.Create<ExplosiveNextMessage>(streamProvider, this.GetPrimaryKey());

            _subscription = await stream.SubscribeAsync(OnNextAsync);
            _compoundStream = StreamFactory.Create<CompoundMessage>(streamProvider, this.GetPrimaryKey());

            await base.OnActivateAsync(cancellationToken);
        }

        private async Task OnNextAsync(ExplosiveNextMessage message, StreamSequenceToken token)
        {
            for (var i = 0; i < 10; i++)
            {
                await _compoundStream.OnNextAsync(new CompoundMessage
                {
                    Data = message.Data,
                    Text = message.Text,
                });
            }
        }
    }
}