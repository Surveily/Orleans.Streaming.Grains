// <copyright file="OneToMany.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Orleans.Hosting;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streaming.Grains.Services;
using Orleans.Streaming.Grains.State;
using Orleans.Streaming.Grains.Streams;
using Orleans.Streaming.Grains.Test;
using Orleans.Streaming.Grains.Tests.Streams.Grains;
using Orleans.Streaming.Grains.Tests.Streams.Messages;
using Should;

namespace Orleans.Streaming.Grains.Tests.Streams.Scenarios
{
    public class OneToMany
    {
        public class Config : BaseGrainTestConfig, IDisposable
        {
            protected Mock<IProcessor> processor = new Mock<IProcessor>();
            private bool _isDisposed;

            public Config()
             : base(false)
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

        public abstract class BaseOneToManyTest : BaseGrainTest<Config>
        {
            protected IOptions<GrainsOptions> settings;

            protected Mock<IProcessor> Processor { get; set; }

            public override void Prepare()
            {
                Processor = Container.GetService<Mock<IProcessor>>();
                settings = Container.GetService<IOptions<GrainsOptions>>();

                base.Prepare();
            }
        }

        public class When_Sending_Compound_Message_One_To_Many : BaseOneToManyTest
        {
            protected string resultText;
            protected string expectedText = "text";

            protected byte[] resultData;
            protected byte[] expectedData = new byte[1024];

            public override void Prepare()
            {
                base.Prepare();

                Processor!.Setup(x => x.Process(It.IsAny<string>()))
                          .Callback<string>(x => resultText = x);

                Processor!.Setup(x => x.Process(It.IsAny<byte[]>()))
                          .Callback<byte[]>(x => resultData = x);

                for (var i = 0; i < 1024; i++)
                {
                    expectedData[i] = Convert.ToByte(i % 2);
                }
            }

            public override async Task Act()
            {
                var grain = Subject.GetGrain<IEmitterGrain>(Guid.NewGuid());

                for (var i = 0; i < 10; i++)
                {
                    await grain.SendAsync(expectedText, expectedData);
                }
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
        }

        public class When_Sending_Explosive_Message_One_To_Many : BaseOneToManyTest
        {
            protected string resultText;
            protected string expectedText = "text";

            protected byte[] resultData;
            protected byte[] expectedData = new byte[1024];

            public override void Prepare()
            {
                base.Prepare();

                Processor!.Setup(x => x.Process(It.IsAny<string>()))
                          .Callback<string>(x => resultText = x);

                Processor!.Setup(x => x.Process(It.IsAny<byte[]>()))
                          .Callback<byte[]>(x => resultData = x);

                for (var i = 0; i < 1024; i++)
                {
                    expectedData[i] = Convert.ToByte(i % 2);
                }
            }

            public override async Task Act()
            {
                var grain = Subject.GetGrain<IEmitterGrain>(Guid.NewGuid());

                await grain.ExplosiveAsync(expectedText, expectedData);
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
        }

        public class When_Sending_Broadcast_Message_One_To_Many : BaseOneToManyTest
        {
            protected string resultText;
            protected string expectedText = "text";

            protected byte[] resultData;
            protected byte[] expectedData = new byte[1024];

            public override void Prepare()
            {
                base.Prepare();

                Processor!.Setup(x => x.Process(It.IsAny<string>()))
                          .Callback<string>(x => resultText = x);

                Processor!.Setup(x => x.Process(It.IsAny<byte[]>()))
                          .Callback<byte[]>(x => resultData = x);

                for (var i = 0; i < 1024; i++)
                {
                    expectedData[i] = Convert.ToByte(i % 2);
                }
            }

            public override async Task Act()
            {
                var grain = Subject.GetGrain<IEmitterGrain>(Guid.NewGuid());

                for (var i = 0; i < 10; i++)
                {
                    await grain.BroadcastAsync(expectedText, expectedData);
                }
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
        }

        public class When_Sending_Broadcast_Message_One_To_Many_Error : BaseOneToManyTest
        {
            protected TransactionGrainState state;

            protected string resultText;
            protected string expectedText = "text";

            protected byte[] resultData;
            protected byte[] expectedData = new byte[1024];

            public override void Prepare()
            {
                base.Prepare();

                Processor!.Setup(x => x.Process(It.IsAny<string>()))
                          .Throws<Exception>();

                Processor!.Setup(x => x.Process(It.IsAny<byte[]>()))
                          .Throws<Exception>();

                for (var i = 0; i < 1024; i++)
                {
                    expectedData[i] = Convert.ToByte(i % 2);
                }
            }

            public override async Task Act()
            {
                var grain = Subject.GetGrain<IEmitterGrain>(Guid.NewGuid());
                var transaction = Subject.GetGrain<ITransactionGrain>("broadcastmessage-0");

                await grain.BroadcastAsync(expectedText, expectedData);

                state = await transaction.GetStateAsync();
            }

            [Test]
            public void It_Should_Deliver_Text()
            {
                Processor!.Verify(x => x.Process(expectedText), Times.Exactly(4));
            }

            [Test]
            public void It_Should_Not_Deliver_Expected_Text()
            {
                resultText.ShouldBeNull();
            }

            [Test]
            public void It_Should_Deliver_Data()
            {
                Processor!.Verify(x => x.Process(expectedData), Times.Exactly(4));
            }

            [Test]
            public void It_Should_Not_Deliver_Expected_Data()
            {
                resultData.ShouldBeNull();
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

            [Test]
            public void State_Should_Have_Transaction_Counts_Empty()
            {
                state.TransactionCounts.ShouldBeEmpty();
            }
        }

        /* TODO Retry
        public class When_Sending_Broadcast_Message_One_To_Many_Error : BaseOneToManyTest
        {
            protected TimeSpan wait = TimeSpan.FromSeconds(60);

            protected string resultText;
            protected Stopwatch timerText;
            protected string expectedText = "text";

            protected byte[] resultData;
            protected Stopwatch timerData;
            protected byte[] expectedData = new byte[1024];

            public override void Prepare()
            {
                base.Prepare();

                Processor!.Setup(x => x.Process(It.IsAny<string>()))
                          .Callback<string>(x =>
                          {
                              timerText ??= Stopwatch.StartNew();

                              if (timerText.Elapsed < wait)
                              {
                                  throw new Exception();
                              }
                              else
                              {
                                  resultText = x;
                              }
                          });

                Processor!.Setup(x => x.Process(It.IsAny<byte[]>()))
                          .Callback<byte[]>(x =>
                          {
                              timerData ??= Stopwatch.StartNew();

                              if (timerData.Elapsed < wait)
                              {
                                  throw new Exception();
                              }
                              else
                              {
                                  resultData = x;
                              }
                          });

                for (var i = 0; i < 1024; i++)
                {
                    expectedData[i] = Convert.ToByte(i % 2);
                }
            }

            public override async Task Act()
            {
                var grain = Subject.GetGrain<IEmitterGrain>(Guid.NewGuid());
                var transaction = Subject.GetGrain<ITransactionGrain>(nameof(BroadcastMessage));

                await grain.BroadcastAsync(expectedText, expectedData);

                var state = await transaction.GetStateAsync();
            }

            [Test]
            public void It_Should_Deliver_Text()
            {
                Processor!.Verify(x => x.Process(expectedText), Times.AtLeast(2));
            }

            [Test]
            public void It_Should_Deliver_Expected_Text()
            {
                expectedText.ShouldEqual(resultText);
            }

            [Test]
            public void It_Should_Deliver_Data()
            {
                Processor!.Verify(x => x.Process(expectedData), Times.AtLeast(2));
            }

            [Test]
            public void It_Should_Deliver_Expected_Data()
            {
                expectedData.ShouldEqual(resultData);
            }
        }*/
    }
}