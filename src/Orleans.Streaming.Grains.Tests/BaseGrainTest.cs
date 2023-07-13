// <copyright file="BaseGrainTest.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System.Diagnostics;
using NATS.Client;
using NATS.Client.JetStream;
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
        public const string NatsConnection = "nats://nats:4222";

        private readonly AsyncRetryPolicy _retryPolicy;

        public BaseGrainTest()
        {
            var builder = new TestClusterBuilder(1);
            builder.AddSiloBuilderConfigurator<T>();
            builder.AddClientBuilderConfigurator<T>();
            Subject = builder.Build();

            _retryPolicy = Policy.Handle<OrleansMessageRejectionException>()
                                 .WaitAndRetryAsync(10, f => TimeSpan.FromSeconds(5));
        }

        public T Config { get; }

        public TestCluster Subject { get; }

        public IServiceProvider Container
        {
            get
            {
                var siloHandle = Subject.Primary as Orleans.TestingHost.InProcessSiloHandle;

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
                await Subject.DeployAsync();
                await Subject.WaitForLivenessToStabilizeAsync();
            });

            Prepare();

            await Act();
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            await Subject.StopAllSilosAsync();
            await Subject.DisposeAsync();
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