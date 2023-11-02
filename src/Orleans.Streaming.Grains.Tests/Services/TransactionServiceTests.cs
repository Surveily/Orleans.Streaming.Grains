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
            protected int item;
            protected Immutable<int> immutableItem;
            protected Mock<IClusterClient> client;
            protected Mock<ITransactionGrain<int>> transaction;
            protected Mock<ITransactionItemGrain<int>> message;

            public BaseTransactionServiceTest()
            {
                item = 100;
                client = new Mock<IClusterClient>();
                transaction = new Mock<ITransactionGrain<int>>();
                message = new Mock<ITransactionItemGrain<int>>();

                client.Setup(x => x.GetGrain<ITransactionItemGrain<int>>(It.IsAny<Guid>(), null))
                      .Callback<Guid, string>((id, _) => itemId = id)
                      .Returns(message.Object);

                client.Setup(x => x.GetGrain<ITransactionGrain<int>>("1", null))
                      .Returns(transaction.Object);

                message.Setup(x => x.SetAsync(It.IsAny<Immutable<int>>()))
                       .Callback<Immutable<int>>(x => immutableItem = x)
                       .Returns(Task.CompletedTask);

                transaction.Setup(x => x.PostAsync(itemId, It.IsAny<int>()))
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
                message.Verify(x => x.SetAsync(immutableItem), Times.Once);
            }

            [Test]
            public void It_Should_Post_Id()
            {
                transaction.Verify(x => x.PostAsync(itemId, item), Times.Once);
            }
        }

        public class WhenPopingEmpty : BaseTransactionServiceTest
        {
            protected (Guid Id, Immutable<int> Item)? result;

            public override async Task SetupAsync()
            {
                await base.SetupAsync();

                result = await Subject.PopAsync("1");
            }

            [Test]
            public void It_Should_Return_Null()
            {
                result.ShouldBeNull();
            }

            [Test]
            public void It_Should_Pop()
            {
                transaction.Verify(x => x.PopAsync(), Times.Once);
            }
        }

        public class WhenPopingSingle : BaseTransactionServiceTest
        {
            protected (Guid Id, Immutable<int> Item)? result;

            public override async Task SetupAsync()
            {
                transaction.Setup(x => x.PopAsync())
                           .ReturnsAsync(() => itemId);

                message.Setup(x => x.GetAsync())
                       .ReturnsAsync(immutableItem);

                await base.SetupAsync();
                await Subject.PostAsync(item, false, "1");

                result = await Subject.PopAsync("1");
            }

            [Test]
            public void It_Should_Return()
            {
                result.HasValue.ShouldBeTrue();
            }

            [Test]
            public void It_Should_Return_Id()
            {
                result.Value.Id.ShouldEqual(itemId);
            }

            [Test]
            public void It_Should_Return_Item()
            {
                result.Value.Item.ShouldEqual(immutableItem);
            }

            [Test]
            public void It_Should_Get_Item()
            {
                client.Verify(x => x.GetGrain<ITransactionItemGrain<int>>(itemId, null), Times.Exactly(2));
            }

            [Test]
            public void It_Should_Get_Transaction()
            {
                client.Verify(x => x.GetGrain<ITransactionGrain<int>>("1", null), Times.Exactly(2));
            }

            [Test]
            public void It_Should_Set_Item()
            {
                message.Verify(x => x.SetAsync(immutableItem), Times.Once);
            }

            [Test]
            public void It_Should_Post_Id()
            {
                transaction.Verify(x => x.PostAsync(itemId, item), Times.Once);
            }

            [Test]
            public void It_Should_Pop()
            {
                transaction.Verify(x => x.PopAsync(), Times.Once);
            }
        }
    }
}