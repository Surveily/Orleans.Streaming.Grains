// <copyright file="BaseGrainTestConfig.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers.Streams.Common;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streaming.Grains.Extensions;
using Orleans.Streaming.Grains.Services;
using Orleans.Streaming.Grains.Streams;
using Orleans.Streaming.Grains.Tests.Streams.Messages;
using Orleans.TestingHost;
using Serilog;
using Serilog.Extensions.Logging;

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
            if (_fireAndForget)
            {
                siloBuilder.ConfigureServices(Configure)
                           .ConfigureServices(ConfigureInner)
                           .AddMemoryGrainStorageAsDefault()
                           .AddMemoryGrainStorage(name: "PubSubStore")
                           .AddGrainsStreams(name: "Default",
                                             queueCount: 1,
                                             retry: TimeSpan.FromSeconds(1),
                                             poison: TimeSpan.FromSeconds(3));
            }
            else
            {
#pragma warning disable CS0618
                siloBuilder.ConfigureServices(Configure)
                           .ConfigureServices(ConfigureInner)
                           .AddMemoryGrainStorageAsDefault()
                           .AddMemoryGrainStorage(name: "PubSubStore")
                           .AddGrainsStreamsForTests(name: "Default",
                                                     queueCount: 3,
                                                     retry: TimeSpan.FromSeconds(1),
                                                     poison: TimeSpan.FromSeconds(3));
#pragma warning restore CS0618
            }
        }

        public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
        {
        }

        private void ConfigureInner(IServiceCollection services)
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Debug()
                .WriteTo.Console()
                .CreateLogger();

            services.AddLogging(l => l.AddSerilog(logger, dispose: true));
        }
    }
}