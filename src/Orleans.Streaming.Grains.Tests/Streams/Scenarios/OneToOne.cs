// <copyright file="OneToOne.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NATS.Client;
using NATS.Client.JetStream;
using NUnit.Framework;
using Orleans.Hosting;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streaming.Grains.Services;
using Orleans.Streaming.Grains.Streams;
using Orleans.Streaming.Grains.Test;
using Orleans.Streaming.Grains.Tests.Streams.Grains;
using Orleans.Streaming.Grains.Tests.Streams.Messages;

namespace Orleans.Streaming.NATS.Test.Scenarios
{
    public class OneToOne
    {
        public class Config : BaseGrainTestConfig, IDisposable
        {
            protected GrainsOptions options = new GrainsOptions();
            protected Mock<IProcessor> processor = new Mock<IProcessor>();
            private bool _isDisposed;

            public override void Configure(IServiceCollection services)
            {
                services.AddSingleton(options);
                services.AddSingleton(processor);
                services.AddSingleton(processor.Object);
                services.AddSingleton<ITransactionService, TransactionService>();
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

        public abstract class BaseOneToOneTest : BaseGrainTest<Config>
        {
            protected Mock<IProcessor> Processor { get; set; }

            public override void Prepare()
            {
                Processor = Container.GetService<Mock<IProcessor>>();

                base.Prepare();
            }
        }

        public class When_Sending_Simple_Message_One_To_One : BaseOneToOneTest
        {
            protected string result;
            protected string expected = "text";

            public override void Prepare()
            {
                base.Prepare();

                Processor!.Setup(x => x.Process(It.IsAny<string>()))
                          .Callback<string>(x => result = x);
            }

            public override async Task Act()
            {
                var grain = Subject.GrainFactory.GetGrain<IEmitterGrain>(Guid.NewGuid());

                await grain.SendAsync(expected);

                await WaitFor(() => result);
            }

            [Test]
            public void It_Should_Deliver_Text()
            {
                Processor!.Verify(x => x.Process(expected), Times.Once);
            }
        }

        public class When_Sending_Blob_Message_One_To_One : BaseOneToOneTest
        {
            protected byte[] result;
            protected byte[] expected = new byte[1024];

            public override void Prepare()
            {
                base.Prepare();

                Processor!.Setup(x => x.Process(It.IsAny<byte[]>()))
                          .Callback<byte[]>(x => result = x);

                for (var i = 0; i < 1024; i++)
                {
                    expected[i] = Convert.ToByte(i % 2);
                }
            }

            public override async Task Act()
            {
                var grain = Subject.GrainFactory.GetGrain<IEmitterGrain>(Guid.NewGuid());

                await grain.SendAsync(expected);

                await WaitFor(() => result);
            }

            [Test]
            public void It_Should_Deliver_Data()
            {
                Processor!.Verify(x => x.Process(expected), Times.Once);
            }
        }
    }
}