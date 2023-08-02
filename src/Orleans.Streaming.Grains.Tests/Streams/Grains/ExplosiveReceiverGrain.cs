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

    [ImplicitStreamSubscription(nameof(ExplosiveMessage))]
    public class ExplosiveSecondReceiverGrain : Grain, IExplosiveReceiverGrain
    {
        private StreamSubscriptionHandle<ExplosiveMessage> _subscription;

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider("Default");
            var stream = StreamFactory.Create<ExplosiveMessage>(streamProvider, this.GetPrimaryKey());

            _subscription = await stream.SubscribeAsync(OnNextAsync);

            await base.OnActivateAsync(cancellationToken);
        }

        private Task OnNextAsync(ExplosiveMessage message, StreamSequenceToken token)
        {
            return Task.CompletedTask;
        }
    }

    [ImplicitStreamSubscription(nameof(ExplosiveNextMessage))]
    public class ExplosiveNextFirstReceiverGrain : Grain, IExplosiveReceiverGrain
    {
        private List<IAsyncStream<CompoundMessage>> _compoundStreams;
        private StreamSubscriptionHandle<ExplosiveNextMessage> _subscription;

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider("Default");

            _compoundStreams = new List<IAsyncStream<CompoundMessage>>();

            foreach (var id in Catalogue.Ids)
            {
                _compoundStreams.Add(StreamFactory.Create<CompoundMessage>(streamProvider, id));
            }

            var stream = StreamFactory.Create<ExplosiveNextMessage>(streamProvider, this.GetPrimaryKey());

            _subscription = await stream.SubscribeAsync(OnNextAsync);

            await base.OnActivateAsync(cancellationToken);
        }

        private async Task OnNextAsync(ExplosiveNextMessage message, StreamSequenceToken token)
        {
            var tasks = new List<Task>();

            foreach (var compoundStream in _compoundStreams)
            {
                tasks.Add(compoundStream.OnNextAsync(new CompoundMessage
                {
                    Data = message.Data,
                    Text = message.Text,
                }));
            }

            await Task.WhenAll(tasks);
        }
    }

    [ImplicitStreamSubscription(nameof(ExplosiveNextMessage))]
    public class ExplosiveNextSecondReceiverGrain : Grain, IExplosiveReceiverGrain
    {
        private List<IAsyncStream<CompoundMessage>> _compoundStreams;
        private StreamSubscriptionHandle<ExplosiveNextMessage> _subscription;

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var streamProvider = this.GetStreamProvider("Default");

            _compoundStreams = new List<IAsyncStream<CompoundMessage>>();

            foreach (var id in Catalogue.Ids)
            {
                _compoundStreams.Add(StreamFactory.Create<CompoundMessage>(streamProvider, id));
            }

            var stream = StreamFactory.Create<ExplosiveNextMessage>(streamProvider, this.GetPrimaryKey());

            _subscription = await stream.SubscribeAsync(OnNextAsync);

            await base.OnActivateAsync(cancellationToken);
        }

        private async Task OnNextAsync(ExplosiveNextMessage message, StreamSequenceToken token)
        {
            var tasks = new List<Task>();

            foreach (var compoundStream in _compoundStreams)
            {
                tasks.Add(compoundStream.OnNextAsync(new CompoundMessage
                {
                    Data = message.Data,
                    Text = message.Text,
                }));
            }

            await Task.WhenAll(tasks);
        }
    }

    public class Catalogue
    {
        public static List<Guid> Ids = new List<Guid>
        {
            new Guid("28C4E45A-8EFA-44EB-990D-0BAB1801A93A"),
            new Guid("DFB27D29-A5B7-439E-8B84-B0BE3C4F1EF3"),
            new Guid("9F1FE184-038C-486F-A9BB-5423AE9F01B1"),
            new Guid("9D0C97EC-80AD-43FF-B99E-695BAB08C8A8"),
            new Guid("7800A671-9144-4360-9EC3-D06E26BAFB78"),
            new Guid("00A53827-9853-4182-AB5F-6C1826DA210D"),
            new Guid("A5CB9B6A-09FD-4C6F-AE67-616A49E1CCEE"),
            new Guid("4D29994E-F470-4F97-A7A0-69F80AC30DF1"),
            new Guid("62026CE9-4841-4207-8DFA-7B1B9EA5E6FC"),
            new Guid("EB72923F-B866-4781-BE64-33FF4166B720"),
        };
    }
}