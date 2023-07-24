// <copyright file="TransactionTests.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Orleans;
using Orleans.Concurrency;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streaming.Grains.Services;
using Orleans.Streaming.Grains.Streams;
using Orleans.Streaming.Grains.Test;
using Should;

namespace Orleans.Streaming.Grains.Tests.Grains
{
    public class TransactionTests
    {
        public class Config : BaseGrainTestConfig
        {
            private GrainsOptions _settings = new GrainsOptions();

            public override void Configure(IServiceCollection services)
            {
                services.AddSingleton(f => _settings);
                services.AddSingleton<ITransactionService, TransactionService>();
            }
        }

        public abstract class BaseTransactionTest : BaseGrainTest<Config>
        {
            protected IClusterClient client;
            protected GrainsOptions settings;
            protected ITransactionService service;

            protected Queue<Guid> queue;
            protected Queue<Guid> poison;
            protected Dictionary<Guid, DateTimeOffset> transactions;

            public override void Prepare()
            {
                client = Container.GetService<IClusterClient>();
                settings = Container.GetService<GrainsOptions>();
                service = Container.GetService<ITransactionService>();
            }
        }

        public class WhenPoppingEmpty : BaseTransactionTest
        {
            protected (Guid Id, Immutable<int> Item)? result;

            public override async Task Act()
            {
                result = await service.PopAsync<int>();
            }

            [Test]
            public void It_Should_Return_Null()
            {
                result.ShouldBeNull();
            }
        }

        public abstract class BaseWhenPoppingSingle : BaseTransactionTest
        {
            protected (Guid Id, Immutable<int> Item)? result;

            public override void Prepare()
            {
                base.Prepare();

                settings.Timeout = TimeSpan.FromSeconds(2);
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
                await service.PostAsync(new Immutable<int>(100), false);

                result = await service.PopAsync<int>();

                var transaction = client.GetGrain<ITransactionGrain>(typeof(int).Name);

                (queue, poison, transactions) = await transaction.GetStateAsync();
            }

            [Test]
            public void State_Should_Have_Poison_Empty()
            {
                poison.ShouldBeEmpty();
            }

            [Test]
            public void State_Should_Have_Queue_Empty()
            {
                queue.ShouldBeEmpty();
            }

            [Test]
            public void State_Should_Have_Transactions_Single()
            {
                transactions.Count.ShouldEqual(1);
            }
        }

        public class WhenPoppingSingleTimeout : BaseWhenPoppingSingle
        {
            public override async Task Act()
            {
                await service.PostAsync(new Immutable<int>(100), false);

                result = await service.PopAsync<int>();

                await Task.Delay(TimeSpan.FromSeconds(3));

                var transaction = client.GetGrain<ITransactionGrain>(typeof(int).Name);

                (queue, poison, transactions) = await transaction.GetStateAsync();
            }

            [Test]
            public void State_Should_Have_Poison_Empty()
            {
                poison.ShouldBeEmpty();
            }

            [Test]
            public void State_Should_Have_Queue_Empty()
            {
                queue.Count.ShouldEqual(1);
            }

            [Test]
            public void State_Should_Have_Transactions_Single()
            {
                transactions.ShouldBeEmpty();
            }
        }

        public class WhenPoppingSingleAfterComplete : BaseWhenPoppingSingle
        {
            protected (Guid Id, Immutable<int> Item)? result2;

            public override async Task Act()
            {
                await service.PostAsync(new Immutable<int>(100), false);

                result = await service.PopAsync<int>();

                await service.CompleteAsync<int>(result.Value.Id, true);

                result2 = await service.PopAsync<int>();

                var transaction = client.GetGrain<ITransactionGrain>(typeof(int).Name);

                (queue, poison, transactions) = await transaction.GetStateAsync();
            }

            [Test]
            public void It_Should_Return_Second_Null()
            {
                result2.ShouldBeNull();
            }

            [Test]
            public void State_Should_Have_Poison_Empty()
            {
                poison.ShouldBeEmpty();
            }

            [Test]
            public void State_Should_Have_Queue_Empty()
            {
                queue.ShouldBeEmpty();
            }

            [Test]
            public void State_Should_Have_Transactions_Empty()
            {
                transactions.ShouldBeEmpty();
            }
        }

        public class WhenPoppingSingleAfterCompletePoison : BaseWhenPoppingSingle
        {
            protected (Guid Id, Immutable<int> Item)? result2;

            public override async Task Act()
            {
                await service.PostAsync(new Immutable<int>(100), false);

                result = await service.PopAsync<int>();

                await service.CompleteAsync<int>(result.Value.Id, false);

                result2 = await service.PopAsync<int>();

                var transaction = client.GetGrain<ITransactionGrain>(typeof(int).Name);

                (queue, poison, transactions) = await transaction.GetStateAsync();
            }

            [Test]
            public void It_Should_Return_Second_Null()
            {
                result2.ShouldBeNull();
            }

            [Test]
            public void State_Should_Have_Poison_Single()
            {
                poison.Count.ShouldEqual(1);
            }

            [Test]
            public void State_Should_Have_Queue_Empty()
            {
                queue.ShouldBeEmpty();
            }

            [Test]
            public void State_Should_Have_Transactions_Empty()
            {
                transactions.ShouldBeEmpty();
            }
        }
    }
}