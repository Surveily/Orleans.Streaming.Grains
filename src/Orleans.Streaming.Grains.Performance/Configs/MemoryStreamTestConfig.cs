// <copyright file="MemoryStreamTestConfig.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Streaming.Grains.Extensions;
using Orleans.Streaming.Grains.Tests.Streams.Messages;
using Orleans.TestingHost;

namespace Orleans.Streaming.Grains.Performance.Configs
{
    public abstract class MemoryStreamTestConfig : ISiloConfigurator, IClientBuilderConfigurator
    {
        public abstract void Configure(IServiceCollection services);

        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.ConfigureServices(Configure)
                       .AddMemoryGrainStorageAsDefault()
                       .AddMemoryGrainStorage(ProviderConstants.DEFAULT_PUBSUB_PROVIDER_NAME)
                       .AddMemoryStreams(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);
        }

        public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
        {
        }
    }

    public class MemoryStreamConfig : MemoryStreamTestConfig, IDisposable
    {
        protected Mock<IProcessor> processor = new Mock<IProcessor>();
        private bool _isDisposed;

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
}