// <copyright file="TransactionTests.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Orleans;
using Orleans.Concurrency;
using Orleans.Providers;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streaming.Grains.Services;
using Orleans.Streaming.Grains.State;
using Orleans.Streaming.Grains.Streams;
using Orleans.Streaming.Grains.Test;
using Should;

namespace Orleans.Streaming.Grains.Tests.Grains
{
    public class TransactionTests
    {
        public class Config : BaseGrainTestConfig
        {
            public override void Configure(IServiceCollection services)
            {
                services.AddSingleton<ITransactionService<int>, TransactionService<int>>();
                services.AddSingleton<IMemoryMessageBodySerializer, DefaultMemoryMessageBodySerializer>();
            }
        }

        public abstract class BaseTransactionTest : BaseGrainTest<Config>
        {
            protected IClusterClient client;
            protected TransactionGrainState state;
            protected ITransactionService<int> service;
            protected IOptions<GrainsOptions> settings;
            protected (Guid Id, Immutable<int> Item, long Sequence)? result;

            public override void Prepare()
            {
                client = Container.GetService<IClusterClient>();
                service = Container.GetService<ITransactionService<int>>();
                settings = Container.GetService<IOptions<GrainsOptions>>();
            }
        }

        public class WhenPoppingEmpty : BaseTransactionTest
        {
            public override async Task Act()
            {
                result = await service.PopAsync("1");
            }

            [Test]
            public void It_Should_Return_Null()
            {
                result.ShouldBeNull();
            }
        }

        public abstract class BaseWhenPoppingSingle : BaseTransactionTest
        {
            public override void Prepare()
            {
                base.Prepare();
            }

            [Test]
            public void It_Should_Return()
            {
                result.HasValue.ShouldBeTrue();
            }

            [Test]
            public void It_Should_Return_Item()
            {
                result.Value.Item.Value.ShouldEqual(100);
            }

            [Test]
            public void It_Should_Return_Id()
            {
                result.Value.Id.ShouldNotEqual(Guid.Empty);
            }
        }

        public class WhenPoppingSingle : BaseWhenPoppingSingle
        {
            public override async Task Act()
            {
                await service.PostAsync(new Immutable<int>(100), false, "1");

                result = await service.PopAsync("1");

                var transaction = client.GetGrain<ITransactionGrain<int>>("1");

                state = await transaction.GetStateAsync();
            }

            [Test]
            public void State_Should_Have_Poison_Empty()
            {
                state.Poison.ShouldBeEmpty();
            }

            [Test]
            public void State_Should_Have_Queue_Empty()
            {
                state.Queue.ShouldBeEmpty();
            }

            [Test]
            public void State_Should_Have_Transactions_Single()
            {
                state.Transactions.Count.ShouldEqual(1);
            }
        }

        public class WhenPoppingSingleTimeout : BaseWhenPoppingSingle
        {
            public override async Task Act()
            {
                await service.PostAsync(new Immutable<int>(100), false, "1");

                result = await service.PopAsync("1");

                var transaction = client.GetGrain<ITransactionGrain<int>>("1");

                await Task.Delay(TimeSpan.FromSeconds(2));

                state = await transaction.GetStateAsync();
            }

            [Test]
            public void State_Should_Have_Poison_Empty()
            {
                state.Poison.ShouldBeEmpty();
            }

            [Test]
            public void State_Should_Have_Queue_One()
            {
                state.Queue.Count.ShouldEqual(1);
            }

            [Test]
            public void State_Should_Have_Transactions_Empty()
            {
                state.Transactions.Count.ShouldEqual(1);
            }
        }

        public class WhenPoppingSingleAfterComplete : BaseWhenPoppingSingle
        {
            protected (Guid Id, Immutable<int> Item, long Sequence)? result2;

            public override async Task Act()
            {
                await service.PostAsync(new Immutable<int>(100), false, "1");

                result = await service.PopAsync("1");

                await service.CompleteAsync(result.Value.Id, true, "1");

                result2 = await service.PopAsync("1");

                var transaction = client.GetGrain<ITransactionGrain<int>>("1");

                state = await transaction.GetStateAsync();
            }

            [Test]
            public void It_Should_Return_Second_Null()
            {
                result2.ShouldBeNull();
            }

            [Test]
            public void State_Should_Have_Poison_Empty()
            {
                state.Poison.ShouldBeEmpty();
            }

            [Test]
            public void State_Should_Have_Queue_Empty()
            {
                state.Queue.ShouldBeEmpty();
            }

            [Test]
            public void State_Should_Have_Transactions_Empty()
            {
                state.Transactions.ShouldBeEmpty();
            }
        }

        public class WhenPoppingSingleAfterCompletePoison : BaseWhenPoppingSingle
        {
            protected (Guid Id, Immutable<int> Item, long Sequence)? result2;

            public override async Task Act()
            {
                await service.PostAsync(new Immutable<int>(100), false, "1");

                result = await service.PopAsync("1");

                await service.CompleteAsync(result.Value.Id, false, "1");

                result2 = await service.PopAsync("1");

                var transaction = client.GetGrain<ITransactionGrain<int>>("1");

                state = await transaction.GetStateAsync();
            }

            [Test]
            public void It_Should_Return_Second_Null()
            {
                result2.ShouldBeNull();
            }

            [Test]
            public void State_Should_Have_Poison_Single()
            {
                state.Poison.Count.ShouldEqual(1);
            }

            [Test]
            public void State_Should_Have_Queue_Empty()
            {
                state.Queue.ShouldBeEmpty();
            }

            [Test]
            public void State_Should_Have_Transactions_Empty()
            {
                state.Transactions.ShouldBeEmpty();
            }
        }
    }
}