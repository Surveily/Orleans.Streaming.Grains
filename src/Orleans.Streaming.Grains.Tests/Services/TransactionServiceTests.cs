// <copyright file="TransactionServiceTests.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Orleans;
using Orleans.Concurrency;
using Orleans.Providers;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streaming.Grains.Services;
using Orleans.Streaming.Grains.Test;
using Should;

namespace Orleans.Streaming.Grains.Tests.Services
{
    public class TransactionServiceTests
    {
        public abstract class BaseTransactionServiceTest : BaseTest<TransactionService<int>>
        {
            protected Guid itemId;
            protected long itemSequence;
            protected Immutable<int> item;
            protected List<(Guid, long)> ids;
            protected Mock<IClusterClient> client;
            protected Mock<ITransactionGrain<int>> transaction;
            protected Mock<ITransactionReaderGrain<int>> reader;
            protected Mock<ITransactionItemGrain<int>> message;
            protected List<(Guid Id, Immutable<int> Item, long Sequence)> items;

            public BaseTransactionServiceTest()
            {
                ids = new List<(Guid, long)>();
                item = new Immutable<int>(100);
                client = new Mock<IClusterClient>();
                transaction = new Mock<ITransactionGrain<int>>();
                message = new Mock<ITransactionItemGrain<int>>();
                reader = new Mock<ITransactionReaderGrain<int>>();
                items = new List<(Guid Id, Immutable<int> Item, long Sequence)>();

                client.Setup(x => x.GetGrain<ITransactionItemGrain<int>>(It.IsAny<Guid>(), null))
                      .Callback<Guid, string>((id, _) =>
                      {
                          itemId = id;
                          itemSequence++;
                          ids.Add((itemId, itemSequence));
                          items.Add((itemId, item, itemSequence));
                      })
                      .Returns(message.Object);

                client.Setup(x => x.GetGrain<ITransactionGrain<int>>("1", null))
                      .Returns(transaction.Object);

                client.Setup(x => x.GetGrain<ITransactionReaderGrain<int>>("1", null))
                      .Returns(reader.Object);

                message.Setup(x => x.SetAsync(It.IsAny<Immutable<int>>()))
                       .Returns(Task.CompletedTask);

                reader.Setup(x => x.GetAsync(ids))
                      .ReturnsAsync(new Immutable<List<(Guid Id, Immutable<int> Item, long Sequence)>>(items));

                transaction.Setup(x => x.PopAsync(1))
                           .ReturnsAsync(ids);

                transaction.Setup(x => x.PostAsync(itemId))
                           .Returns(Task.CompletedTask);

                Services.AddSingleton(client.Object);
            }
        }

        public class WhenPosting : BaseTransactionServiceTest
        {
            public override async Task SetupAsync()
            {
                await base.SetupAsync();
                await Subject.PostAsync(item, false, "1");
            }

            [Test]
            public void It_Should_Get_Item()
            {
                client.Verify(x => x.GetGrain<ITransactionItemGrain<int>>(itemId, null), Times.Once);
            }

            [Test]
            public void It_Should_Get_Transaction()
            {
                client.Verify(x => x.GetGrain<ITransactionGrain<int>>("1", null), Times.Once);
            }

            [Test]
            public void It_Should_Set_Item()
            {
                message.Verify(x => x.SetAsync(item), Times.Once);
            }

            [Test]
            public void It_Should_Post_Id()
            {
                transaction.Verify(x => x.PostAsync(itemId), Times.Once);
            }
        }

        public class WhenPopingEmpty : BaseTransactionServiceTest
        {
            protected List<(Guid Id, Immutable<int> Item, long Sequence)> results;

            public override async Task SetupAsync()
            {
                await base.SetupAsync();

                results = await Subject.PopAsync("1", 1);
            }

            [Test]
            public void It_Should_Return_Null()
            {
                results.ShouldBeEmpty();
            }

            [Test]
            public void It_Should_Pop()
            {
                transaction.Verify(x => x.PopAsync(1), Times.Once);
            }
        }

        public class WhenPopingSingle : BaseTransactionServiceTest
        {
            protected List<(Guid Id, Immutable<int> Item, long Sequence)> results;

            public override async Task SetupAsync()
            {
                message.Setup(x => x.GetAsync())
                       .ReturnsAsync(item);

                await base.SetupAsync();
                await Subject.PostAsync(item, false, "1");

                results = await Subject.PopAsync("1", 1);
            }

            [Test]
            public void It_Should_Return()
            {
                results.ShouldNotBeEmpty();
            }

            [Test]
            public void It_Should_Return_Id()
            {
                results.First().Id.ShouldEqual(itemId);
            }

            [Test]
            public void It_Should_Return_Item()
            {
                results.First().Item.ShouldEqual(item);
            }

            [Test]
            public void It_Should_Get_Reader()
            {
                client.Verify(x => x.GetGrain<ITransactionReaderGrain<int>>("1", null), Times.Exactly(1));
            }

            [Test]
            public void It_Should_Get_Item()
            {
                client.Verify(x => x.GetGrain<ITransactionItemGrain<int>>(itemId, null), Times.Exactly(1));
            }

            [Test]
            public void It_Should_Get_Transaction()
            {
                client.Verify(x => x.GetGrain<ITransactionGrain<int>>("1", null), Times.Exactly(2));
            }

            [Test]
            public void It_Should_Set_Item()
            {
                message.Verify(x => x.SetAsync(item), Times.Once);
            }

            [Test]
            public void It_Should_Post_Id()
            {
                transaction.Verify(x => x.PostAsync(itemId), Times.Once);
            }

            [Test]
            public void It_Should_Pop()
            {
                transaction.Verify(x => x.PopAsync(1), Times.Once);
            }
        }
    }
}