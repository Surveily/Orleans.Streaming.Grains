// <copyright file="ISiloBuilderExtensions.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streaming.Grains.Services;
using Orleans.Streaming.Grains.Streams;
using Orleans.Streams;

namespace Orleans.Streaming.Grains.Extensions
{
    public static class ISiloBuilderExtensions
    {
        /// <summary>
        /// Register Grains Stream Provider with the SiloBuilder.
        /// </summary>
        /// <param name="builder">SiloBuilder to register with.</param>
        /// <param name="name">Name of the provider.</param>
        /// <param name="queueCount">How many queues should be load balanced with.</param>
        /// <param name="retry">How many times should a failed message be retried before going to Poison queue.</param>
        /// <returns>SiloBuilder registered.</returns>
        public static ISiloBuilder AddGrainsStreams(this ISiloBuilder builder, string name, int queueCount, int retry)
        {
            return builder.AddGrainsStreams(name, queueCount, true, TimeSpan.FromMinutes(5), retry);
        }

        /// <summary>
        /// Register Grains Stream Provider with the SiloBuilder FOR TESTS ONLY.
        /// </summary>
        /// <param name="builder">SiloBuilder to register with.</param>
        /// <param name="name">Name of the provider.</param>
        /// <param name="queueCount">How many queues should be load balanced with.</param>
        /// <param name="retry">How many times should a failed message be retried before going to Poison queue.</param>
        /// <param name="messagesForTests">All contracts that are sent in streams.</param>
        /// <returns>SiloBuilder registered.</returns>
        [Obsolete("This method is for use with TestCluster only. Use `pragma warning disable CS0618` to use it without warnings.")]
        public static ISiloBuilder AddGrainsStreamsForTests(this ISiloBuilder builder, string name, int queueCount, int retry, params Type[] messagesForTests)
        {
            return builder.AddGrainsStreams(name, queueCount, false, TimeSpan.FromSeconds(2), retry, messagesForTests);
        }

        private static ISiloBuilder AddGrainsStreams(this ISiloBuilder builder, string name, int queueCount, bool fireAndForgetDelivery, TimeSpan timeout, int retry, params Type[] messagesForTests)
        {
            return builder.ConfigureServices(services =>
                          {
                              services.AddSingleton<ITransactionService, TransactionService>();

                              if (!fireAndForgetDelivery)
                              {
                                  services.AddSingletonNamedService<IStreamQueueBalancer>(name, (f, n) => new GrainsQueueBalancer());
                                  services.AddSingletonNamedService<IStreamQueueMapper>(name, (f, n) => new GrainsQueueMapper(messagesForTests, queueCount));
                              }
                          })
                          .Configure<GrainsOptions>(options =>
                          {
                              options.Retry = retry;
                              options.Timeout = timeout;
                              options.FireAndForgetDelivery = fireAndForgetDelivery;
                          })
                          .AddPersistentStreams(name, GrainsQueueAdapterFactory.Create, config =>
                          {
                              config.Configure<GrainsOptions>(options =>
                              {
                                  options.Configure(x => x.Retry = retry);
                                  options.Configure(x => x.Timeout = timeout);
                                  options.Configure(x => x.FireAndForgetDelivery = fireAndForgetDelivery);
                              });
                              if (fireAndForgetDelivery)
                              {
                                  config.Configure<HashRingStreamQueueMapperOptions>(options =>
                                  {
                                      options.Configure(x => x.TotalQueueCount = queueCount);
                                  });
                              }
                          });
        }
    }
}