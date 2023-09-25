// <copyright file="OneToMany.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Concurrency;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streaming.Grains.Performance.Configs;
using Orleans.Streaming.Grains.Services;
using Orleans.Streaming.Grains.Streams;
using Orleans.Streaming.Grains.Tests.Streams.Grains;

namespace Orleans.Streaming.Grains.Performance.Scenarios
{
    public class OneToMany
    {
        public class OneToManyBasicTest : BenchmarkBaseSilo<BasicConfig>
        {
            protected string expectedText = "text";
            protected byte[] expectedData = new byte[1024];

            [Benchmark]
            [GcConcurrent]
            [GcServer(true)]
            public async Task BroadcastAsync()
            {
                var grain = Client.GetGrain<IEmitterGrain>(Guid.NewGuid());

                await grain.BroadcastAsync(expectedText, expectedData);
            }

            [Benchmark]
            [GcConcurrent]
            [GcServer(true)]
            public async Task CompoundAsync()
            {
                var grain = Client.GetGrain<IEmitterGrain>(Guid.NewGuid());

                await grain.CompoundAsync(expectedText, expectedData);
            }

            [Benchmark]
            [GcConcurrent]
            [GcServer(true)]
            public async Task ExplosiveAsync()
            {
                var grain = Client.GetGrain<IEmitterGrain>(Guid.NewGuid());

                await grain.ExplosiveAsync(expectedText, expectedData);
            }
        }

        public class OneToManyPersistentTest : BenchmarkBaseSilo<PersistentConfig>
        {
            protected string expectedText = "text";
            protected byte[] expectedData = new byte[1024];

            [Benchmark]
            [GcConcurrent]
            [GcServer(true)]
            public async Task BroadcastAsync()
            {
                var grain = Client.GetGrain<IEmitterGrain>(Guid.NewGuid());

                await grain.BroadcastAsync(expectedText, expectedData);
            }

            [Benchmark]
            [GcConcurrent]
            [GcServer(true)]
            public async Task CompoundAsync()
            {
                var grain = Client.GetGrain<IEmitterGrain>(Guid.NewGuid());

                await grain.CompoundAsync(expectedText, expectedData);
            }

            [Benchmark]
            [GcConcurrent]
            [GcServer(true)]
            public async Task ExplosiveAsync()
            {
                var grain = Client.GetGrain<IEmitterGrain>(Guid.NewGuid());

                await grain.ExplosiveAsync(expectedText, expectedData);
            }
        }
    }
}