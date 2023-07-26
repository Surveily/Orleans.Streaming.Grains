// <copyright file="BaseGrainTest.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System.Diagnostics;
using NUnit.Framework;
using Orleans.Runtime;
using Orleans.TestingHost;
using Polly;
using Polly.Retry;
#pragma warning disable CS0618

namespace Orleans.Streaming.Grains.Test
{
    public abstract class BaseGrainTest<T>
        where T : ISiloConfigurator, IClientBuilderConfigurator, new()
    {
        public const int QueueNumber = 8;

        private readonly TestCluster _cluster;
        private readonly AsyncRetryPolicy _retryPolicy;

        public BaseGrainTest()
        {
            _cluster = new TestClusterBuilder(1).AddSiloBuilderConfigurator<T>()
                                                .AddClientBuilderConfigurator<T>()
                                                .Build();

            _retryPolicy = Policy.Handle<OrleansMessageRejectionException>()
                                 .WaitAndRetryAsync(10, f => TimeSpan.FromSeconds(5));
        }

        public T Config { get; }

        public IClusterClient Subject => _cluster.Client;

        public IServiceProvider Container
        {
            get
            {
                var siloHandle = _cluster.Primary as Orleans.TestingHost.InProcessSiloHandle;

                return siloHandle.SiloHost.Services;
            }
        }

        public virtual void Prepare()
        {
        }

        public abstract Task Act();

        [OneTimeSetUp]
        public async Task SetupAsync()
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _cluster.DeployAsync();
                await _cluster.WaitForLivenessToStabilizeAsync();
            });

            Prepare();

            await Act();
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            await _cluster.StopAllSilosAsync();
            await _cluster.DisposeAsync();
        }

        protected async Task WaitFor(Func<object> subject)
        {
            await WaitFor(subject, TimeSpan.FromSeconds(3));
        }

        protected async Task WaitFor(Func<object> subject, TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                while (subject() == null)
                {
#if DEBUG
                    timeout = TimeSpan.FromMinutes(2);
#endif
                    if (sw.Elapsed > timeout)
                    {
                        throw new TimeoutException($"Timeout while waiting for subject.");
                    }

                    await Task.Delay(100);
                }
            }
            finally
            {
                sw.Stop();
            }
        }
    }
}