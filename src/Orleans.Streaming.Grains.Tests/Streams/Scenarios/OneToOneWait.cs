// <copyright file="OneToOneWait.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streaming.Grains.Services;
using Orleans.Streaming.Grains.Streams;
using Orleans.Streaming.Grains.Test;
using Orleans.Streaming.Grains.Tests.Streams.Grains;
using Orleans.Streaming.Grains.Tests.Streams.Messages;
using Should;

namespace Orleans.Streaming.Grains.Test.Scenarios
{
    public class OneToOneWait
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
                services.AddSingleton<IMemoryMessageBodySerializer, DefaultMemoryMessageBodySerializer>();
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

        public abstract class BaseOneToOneWaitTest : BaseGrainTest<Config>
        {
            protected Mock<IProcessor> Processor { get; set; }

            public override void Prepare()
            {
                Processor = Container.GetService<Mock<IProcessor>>();

                base.Prepare();
            }
        }

        public class When_Sending_Simple_Message_One_To_One : BaseOneToOneWaitTest
        {
            protected string result;
            protected long resultCounter;
            protected string expected = "text";

            public override void Prepare()
            {
                base.Prepare();

                Processor!.Setup(x => x.Process(It.IsAny<string>()))
                          .Callback<string>(x => result = ++resultCounter == 10 ? x : null);
            }

            public override async Task Act()
            {
                for (var i = 0; i < 10; i++)
                {
                    var grain = Subject.GetGrain<IEmitterGrain>(Guid.NewGuid());

                    await grain.SendAsync(expected);
                }

                await WaitFor(() => result);
            }

            [Test]
            public void It_Should_Deliver()
            {
                Processor!.Verify(x => x.Process(expected), Times.Exactly(10));
            }

            [Test]
            public void It_Should_Deliver_Expected()
            {
                expected.ShouldEqual(result);
            }
        }

        public class When_Sending_Blob_Message_One_To_One : BaseOneToOneWaitTest
        {
            protected byte[] result;
            protected long resultCounter;
            protected byte[] expected = new byte[1024];

            public override void Prepare()
            {
                base.Prepare();

                Processor!.Setup(x => x.Process(It.IsAny<byte[]>()))
                          .Callback<byte[]>(x => result = ++resultCounter == 10 ? x : null);

                for (var i = 0; i < 1024; i++)
                {
                    expected[i] = Convert.ToByte(i % 2);
                }
            }

            public override async Task Act()
            {
                for (var i = 0; i < 10; i++)
                {
                    var grain = Subject.GetGrain<IEmitterGrain>(Guid.NewGuid());

                    await grain.SendAsync(expected);
                }

                await WaitFor(() => result);
            }

            [Test]
            public void It_Should_Deliver()
            {
                Processor!.Verify(x => x.Process(expected), Times.Exactly(10));
            }

            [Test]
            public void It_Should_Deliver_Expected()
            {
                expected.ShouldEqual(result);
            }
        }
    }
}