// <copyright file="OneToManyWait.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streaming.Grains.Services;
using Orleans.Streaming.Grains.Streams;
using Orleans.Streaming.Grains.Test;
using Orleans.Streaming.Grains.Tests.Streams.Grains;
using Orleans.Streaming.Grains.Tests.Streams.Messages;
using Should;

namespace Orleans.Streaming.Grains.Tests.Streams.Scenarios
{
    public class OneToManyWait
    {
        public class Config : BaseGrainTestConfig, IDisposable
        {
            protected Mock<IProcessor> processor = new Mock<IProcessor>();
            private bool _isDisposed;

            public Config()
             : base(true)
            {
            }

            public override void Configure(IServiceCollection services)
            {
                services.AddSingleton(processor);
                services.AddSingleton(processor.Object);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_isDisposed)
                {
                    if (disposing)
                    {
                        /* dispose code here */
                    }

                    _isDisposed = true;
                }
            }
        }

        public abstract class BaseOneToManyWaitTest : BaseGrainTest<Config>
        {
            protected IOptions<GrainsOptions> Settings;

            protected Mock<IProcessor> Processor { get; set; }

            public override void Prepare()
            {
                Processor = Container.GetService<Mock<IProcessor>>();
                Settings = Container.GetService<IOptions<GrainsOptions>>();

                base.Prepare();
            }
        }

        public class When_Sending_Compound_Message_One_To_Many : BaseOneToManyWaitTest
        {
            protected string resultText;
            protected long resultTextCounter;
            protected string expectedText = "text";

            protected byte[] resultData;
            protected long resultDataCounter;
            protected byte[] expectedData = new byte[1024];

            public override void Prepare()
            {
                base.Prepare();

                Processor!.Setup(x => x.Process(It.IsAny<string>()))
                          .Callback<string>(x => resultText = ++resultTextCounter == 10 ? x : null);

                Processor!.Setup(x => x.Process(It.IsAny<byte[]>()))
                          .Callback<byte[]>(x => resultData = ++resultDataCounter == 10 ? x : null);

                for (var i = 0; i < 1024; i++)
                {
                    expectedData[i] = Convert.ToByte(i % 2);
                }
            }

            public override async Task Act()
            {
                for (var i = 0; i < 10; i++)
                {
                    var grain = Subject.GetGrain<IEmitterGrain>(Guid.NewGuid());

                    await grain.SendAsync(expectedText, expectedData);
                }

                await Task.WhenAll(WaitFor(() => resultData), WaitFor(() => resultText));
            }

            [Test]
            public void It_Should_Deliver_Text()
            {
                Processor!.Verify(x => x.Process(expectedText), Times.Exactly(10));
            }

            [Test]
            public void It_Should_Deliver_Expected_Text()
            {
                expectedText.ShouldEqual(resultText);
            }

            [Test]
            public void It_Should_Deliver_Data()
            {
                Processor!.Verify(x => x.Process(expectedData), Times.Exactly(10));
            }

            [Test]
            public void It_Should_Deliver_Expected_Data()
            {
                expectedData.ShouldEqual(resultData);
            }

            [Test]
            public async Task It_Should_Empty_Queue()
            {
                for (var i = 0; i < Settings.Value.QueueCount; i++)
                {
                    var grain = Subject.GetGrain<ITransactionGrain>($"{nameof(CompoundMessage).ToLower()}-{i}");
                    var state = await grain.GetStateAsync();

                    state.Queue.ShouldBeEmpty();
                }
            }

            [Test]
            public async Task It_Should_Empty_Poison()
            {
                for (var i = 0; i < Settings.Value.QueueCount; i++)
                {
                    var grain = Subject.GetGrain<ITransactionGrain>($"{nameof(CompoundMessage).ToLower()}-{i}");
                    var state = await grain.GetStateAsync();

                    state.Poison.ShouldBeEmpty();
                }
            }

            [Test]
            public async Task It_Should_Empty_Transactions()
            {
                for (var i = 0; i < Settings.Value.QueueCount; i++)
                {
                    var grain = Subject.GetGrain<ITransactionGrain>($"{nameof(CompoundMessage).ToLower()}-{i}");
                    var state = await grain.GetStateAsync();

                    state.Transactions.ShouldBeEmpty();
                }
            }
        }

        public class When_Sending_Explosive_Message_One_To_Many : BaseOneToManyWaitTest
        {
            protected string resultText;
            protected long resultTextCounter;
            protected string expectedText = "text";

            protected byte[] resultData;
            protected long resultDataCounter;
            protected byte[] expectedData = new byte[1024];

            public override void Prepare()
            {
                base.Prepare();

                Processor!.Setup(x => x.Process(It.IsAny<string>()))
                          .Callback<string>(x => resultText = ++resultTextCounter == 20 ? x : null);

                Processor!.Setup(x => x.Process(It.IsAny<byte[]>()))
                          .Callback<byte[]>(x => resultData = ++resultDataCounter == 20 ? x : null);

                for (var i = 0; i < 1024; i++)
                {
                    expectedData[i] = Convert.ToByte(i % 2);
                }
            }

            public override async Task Act()
            {
                var grain = Subject.GetGrain<IEmitterGrain>(Guid.NewGuid());

                await grain.ExplosiveAsync(expectedText, expectedData);

                await Task.WhenAll(WaitFor(() => resultData), WaitFor(() => resultText));
            }

            [Test]
            public void It_Should_Deliver_Text()
            {
                Processor!.Verify(x => x.Process(expectedText), Times.Exactly(20));
            }

            [Test]
            public void It_Should_Deliver_Expected_Text()
            {
                expectedText.ShouldEqual(resultText);
            }

            [Test]
            public void It_Should_Deliver_Data()
            {
                Processor!.Verify(x => x.Process(expectedData), Times.Exactly(20));
            }

            [Test]
            public void It_Should_Deliver_Expected_Data()
            {
                expectedData.ShouldEqual(resultData);
            }

            [Test]
            public async Task It_Should_Empty_Queue()
            {
                for (var i = 0; i < Settings.Value.QueueCount; i++)
                {
                    var grain = Subject.GetGrain<ITransactionGrain>($"{nameof(ExplosiveMessage).ToLower()}-{i}");
                    var state = await grain.GetStateAsync();

                    state.Queue.ShouldBeEmpty();
                }
            }

            [Test]
            public async Task It_Should_Empty_Poison()
            {
                for (var i = 0; i < Settings.Value.QueueCount; i++)
                {
                    var grain = Subject.GetGrain<ITransactionGrain>($"{nameof(ExplosiveMessage).ToLower()}-{i}");
                    var state = await grain.GetStateAsync();

                    state.Poison.ShouldBeEmpty();
                }
            }

            [Test]
            public async Task It_Should_Empty_Transactions()
            {
                for (var i = 0; i < Settings.Value.QueueCount; i++)
                {
                    var grain = Subject.GetGrain<ITransactionGrain>($"{nameof(ExplosiveMessage).ToLower()}-{i}");
                    var state = await grain.GetStateAsync();

                    state.Transactions.ShouldBeEmpty();
                }
            }
        }

        public class When_Sending_Broadcast_Message_One_To_Many : BaseOneToManyWaitTest
        {
            protected string resultText;
            protected long resultTextCounter;
            protected string expectedText = "text";

            protected byte[] resultData;
            protected long resultDataCounter;
            protected byte[] expectedData = new byte[1024];

            public override void Prepare()
            {
                base.Prepare();

                Processor!.Setup(x => x.Process(It.IsAny<string>()))
                          .Callback<string>(x => resultText = ++resultTextCounter == 10 ? x : null);

                Processor!.Setup(x => x.Process(It.IsAny<byte[]>()))
                          .Callback<byte[]>(x => resultData = ++resultDataCounter == 10 ? x : null);

                for (var i = 0; i < 1024; i++)
                {
                    expectedData[i] = Convert.ToByte(i % 2);
                }
            }

            public override async Task Act()
            {
                for (var i = 0; i < 10; i++)
                {
                    var grain = Subject.GetGrain<IEmitterGrain>(Guid.NewGuid());

                    await grain.BroadcastAsync(expectedText, expectedData);
                }

                await Task.WhenAll(WaitFor(() => resultData), WaitFor(() => resultText));
            }

            [Test]
            public void It_Should_Deliver_Text()
            {
                Processor!.Verify(x => x.Process(expectedText), Times.Exactly(10));
            }

            [Test]
            public void It_Should_Deliver_Expected_Text()
            {
                expectedText.ShouldEqual(resultText);
            }

            [Test]
            public void It_Should_Deliver_Data()
            {
                Processor!.Verify(x => x.Process(expectedData), Times.Exactly(10));
            }

            [Test]
            public void It_Should_Deliver_Expected_Data()
            {
                expectedData.ShouldEqual(resultData);
            }

            [Test]
            public async Task It_Should_Empty_Queue()
            {
                for (var i = 0; i < Settings.Value.QueueCount; i++)
                {
                    var grain = Subject.GetGrain<ITransactionGrain>($"{nameof(BroadcastMessage).ToLower()}-{i}");
                    var state = await grain.GetStateAsync();

                    state.Queue.ShouldBeEmpty();
                }
            }

            [Test]
            public async Task It_Should_Empty_Poison()
            {
                for (var i = 0; i < Settings.Value.QueueCount; i++)
                {
                    var grain = Subject.GetGrain<ITransactionGrain>($"{nameof(BroadcastMessage).ToLower()}-{i}");
                    var state = await grain.GetStateAsync();

                    state.Poison.ShouldBeEmpty();
                }
            }

            [Test]
            public async Task It_Should_Empty_Transactions()
            {
                for (var i = 0; i < Settings.Value.QueueCount; i++)
                {
                    var grain = Subject.GetGrain<ITransactionGrain>($"{nameof(BroadcastMessage).ToLower()}-{i}");
                    var state = await grain.GetStateAsync();

                    state.Transactions.ShouldBeEmpty();
                }
            }
        }
    }
}