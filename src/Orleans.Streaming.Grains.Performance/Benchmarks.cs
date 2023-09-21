// <copyright file="Benchmarks.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using static Orleans.Streaming.Grains.Tests.Streams.Scenarios.OneToMany;

namespace Orleans.Streaming.Grains.Performance
{
    public class Benchmarks
    {
        [Benchmark]
        [GcServer]
        [GcConcurrent]
        public async Task Scenario1()
        {
            // Implement your benchmark here
            var test = new When_Sending_Compound_Message_One_To_Many();

            await test.SetupAsync();

            test.Prepare();

            await test.Act();
            await test.TearDown();
        }

        /*[Benchmark]
        [GcServer]
        [GcConcurrent]
        public void Scenario2()
        {
            // Implement your benchmark here
        }*/
    }
}
