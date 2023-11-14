// <copyright file="PersistentGrainTestConfig.cs" company="Surveily Sp. z o.o.">
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
    public abstract class PersistentGrainTestConfig : ISiloConfigurator, IClientBuilderConfigurator
    {
        public abstract void Configure(IServiceCollection services);

        public void Configure(ISiloBuilder siloBuilder)
        {
#pragma warning disable CS0618
            siloBuilder.ConfigureServices(Configure)
                       .AddMemoryGrainStorageAsDefault()
                       .AddMemoryGrainStorage(ProviderConstants.DEFAULT_PUBSUB_PROVIDER_NAME)
                       .AddGrainsStreams(name: ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME,
                                         queueCount: 8,
                                         retry: TimeSpan.FromSeconds(1),
                                         poison: TimeSpan.FromSeconds(3));
#pragma warning restore CS0618
        }

        public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
        {
        }
    }

    public class PersistentConfig : PersistentGrainTestConfig, IDisposable
    {
        protected Mock<IProcessor> processor = new Mock<IProcessor>();
        private bool _isDisposed;

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
}