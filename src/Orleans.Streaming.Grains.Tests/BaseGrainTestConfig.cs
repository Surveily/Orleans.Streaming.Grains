// <copyright file="BaseGrainTestConfig.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Streaming.Grains.Streams;
using Orleans.TestingHost;
#pragma warning disable CS0618

namespace Orleans.Streaming.Grains.Test
{
    public abstract class BaseGrainTestConfig : ISiloConfigurator, IClientBuilderConfigurator
    {
        private readonly bool _fireAndForget;

        protected BaseGrainTestConfig(bool fireAndForget = false)
        {
            _fireAndForget = fireAndForget;
        }

        public abstract void Configure(IServiceCollection services);

        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.ConfigureServices(Configure)
                       .AddMemoryGrainStorageAsDefault()
                       .AddMemoryGrainStorage("PubSubStore")
                       .Configure<GrainsOptions>(options =>
                       {
                           options.FireAndForgetDelivery = _fireAndForget;
                       })
                       .Configure<HashRingStreamQueueMapperOptions>(options =>
                       {
                           options.TotalQueueCount = 8;
                       })
                       .AddPersistentStreams("Default", GrainsQueueAdapterFactory.Create, config => config.Configure<GrainsOptions>(options =>
                       {
                       }));
        }

        public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
        {
        }
    }
}
#pragma warning restore CS0618