// <copyright file="OneToMany.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Concurrency;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streaming.Grains.Performance.Configs;
using Orleans.Streaming.Grains.Services;
using Orleans.Streaming.Grains.Streams;
using Orleans.Streaming.Grains.Tests.Streams.Grains;
using Orleans.Streaming.Grains.Tests.Streams.Messages;
using Orleans.TestingHost;

namespace Orleans.Streaming.Grains.Performance.Scenarios
{
    public class OneToMany
    {
        public abstract class BaseOneToManyTest<T> : BenchmarkBaseSilo<T>
          where T : ISiloConfigurator, IClientBuilderConfigurator, new()
        {
            protected string expectedText = "text";
            protected byte[] expectedData = new byte[1024];

            protected Mock<IProcessor> processor;

            [Benchmark]
            [GcConcurrent]
            [GcServer(true)]
            public async Task BroadcastAsync()
            {
                await RunAndWait(1, async () =>
                {
                    var grain = Client.GetGrain<IEmitterGrain>(Guid.NewGuid());

                    await grain.BroadcastAsync(expectedText, expectedData);
                });
            }

            [Benchmark]
            [GcConcurrent]
            [GcServer(true)]
            public async Task CompoundAsync()
            {
                await RunAndWait(1, async () =>
                {
                    var grain = Client.GetGrain<IEmitterGrain>(Guid.NewGuid());

                    await grain.CompoundAsync(expectedText, expectedData);
                });
            }

            [Benchmark]
            [GcConcurrent]
            [GcServer(true)]
            public async Task ExplosiveAsync()
            {
                await RunAndWait(20, async () =>
                {
                    var grain = Client.GetGrain<IEmitterGrain>(Guid.NewGuid());

                    await grain.ExplosiveAsync(expectedText, expectedData);
                });
            }

            protected override void Prepare()
            {
                processor = Container.GetService<Mock<IProcessor>>();
            }

            private async Task RunAndWait(int counter, Func<Task> operation)
            {
                string resultText = null;
                long resultTextCounter = 0;
                byte[] resultData = null;
                long resultDataCounter = 0;

                processor!.Setup(x => x.Process(It.IsAny<string>()))
                          .Callback<string>(x =>
                          {
                              Interlocked.Increment(ref resultTextCounter);

                              resultText = resultTextCounter == counter ? x : null;
                          });

                processor!.Setup(x => x.Process(It.IsAny<byte[]>()))
                          .Callback<byte[]>(x =>
                          {
                              Interlocked.Increment(ref resultDataCounter);

                              resultData = resultDataCounter == counter ? x : null;
                          });

                await operation();

                await Task.WhenAll(WaitFor(() => resultData), WaitFor(() => resultText));
            }
        }

        public class OneToManyGrainsStreamTest : BaseOneToManyTest<GrainsStreamConfig>
        {
        }

        public class OneToManyMemoryStreamTest : BaseOneToManyTest<MemoryStreamConfig>
        {
        }

        public class OneToManyBroadcastChannelTest : BaseOneToManyTest<BroadcastChannelConfig>
        {
        }
    }
}