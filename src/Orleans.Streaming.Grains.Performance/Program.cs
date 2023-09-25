// <copyright file="Program.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Orleans.Streaming.Grains.Performance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = DefaultConfig.Instance;

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                             .Run(args, config);
        }
    }
}