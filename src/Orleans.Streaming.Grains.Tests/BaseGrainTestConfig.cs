// <copyright file="BaseGrainTestConfig.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers.Streams.Common;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streaming.Grains.Services;
using Orleans.Streaming.Grains.Streams;
using Orleans.TestingHost;

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
                       .ConfigureServices(ConfigureRequired)
                       .AddMemoryGrainStorageAsDefault()
                       .AddMemoryGrainStorage("PubSubStore")
                       .AddPersistentStreams("Default", GrainsQueueAdapterFactory.Create, config =>
                       {
                           config.Configure<GrainsOptions>(options =>
                           {
                               options.Configure(x => x.FireAndForgetDelivery = _fireAndForget);
                           });
                           config.Configure<HashRingStreamQueueMapperOptions>(options =>
                           {
                               options.Configure(x => x.TotalQueueCount = 4);
                           });
                       });
        }

        public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
        {
        }

        private void ConfigureRequired(IServiceCollection services)
        {
            services.AddSingleton<ITransactionService, TransactionService>();
        }
    }
}