// <copyright file="BenchmarkBaseSilo.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using Orleans.Runtime;
using Orleans.TestingHost;
using Polly;
using Polly.Retry;

namespace Orleans.Streaming.Grains.Performance
{
    public abstract class BenchmarkBaseSilo<T>
        where T : ISiloConfigurator, IClientBuilderConfigurator, new()
    {
        private readonly TestCluster _cluster;
        private readonly AsyncRetryPolicy _retryPolicy;

        public BenchmarkBaseSilo()
        {
            _cluster = new TestClusterBuilder(1).AddSiloBuilderConfigurator<T>()
                                                .AddClientBuilderConfigurator<T>()
                                                .Build();

            _retryPolicy = Policy.Handle<OrleansMessageRejectionException>()
                                 .WaitAndRetryAsync(10, f => TimeSpan.FromSeconds(5));
        }

        protected IClusterClient Client => _cluster.Client;

        protected IServiceProvider Container
        {
            get
            {
                var siloHandle = _cluster.Primary as InProcessSiloHandle;

                return siloHandle.SiloHost.Services;
            }
        }

        [GlobalSetup]
        public async Task SetupAsync()
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _cluster.DeployAsync();
                await _cluster.WaitForLivenessToStabilizeAsync();
            });

            Prepare();
        }

        [GlobalCleanup]
        public async Task TearDown()
        {
            await _cluster.StopAllSilosAsync();
            await _cluster.DisposeAsync();
        }

        protected virtual void Prepare()
        {
        }

        protected async Task WaitFor(Func<object> subject)
        {
            await WaitFor(subject, TimeSpan.FromSeconds(15));
        }

        protected async Task WaitFor(Func<object> subject, TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                while (subject() == null)
                {
                    if (sw.Elapsed > timeout)
                    {
                        throw new TimeoutException($"Timeout while waiting for subject.");
                    }

                    await Task.Delay(5);
                }
            }
            finally
            {
                sw.Stop();
            }
        }
    }
}