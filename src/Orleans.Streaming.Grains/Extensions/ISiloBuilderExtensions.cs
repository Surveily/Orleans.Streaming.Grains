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
        /// <param name="retry">How long should we wait until the messages is not Completed so it can be put back on the Main queue.</param>
        /// <param name="poison">How long should we wait until the messages is not Completed so it can be put back on the Poison queue.</param>
        /// <returns>SiloBuilder registered.</returns>
        public static ISiloBuilder AddGrainsStreams(this ISiloBuilder builder, string name, int queueCount, TimeSpan retry, TimeSpan poison)
        {
            return builder.AddGrainsStreams(name, queueCount, true, retry, poison);
        }

        /// <summary>
        /// Register Grains Stream Provider with the SiloBuilder FOR TESTS ONLY.
        /// </summary>
        /// <param name="builder">SiloBuilder to register with.</param>
        /// <param name="name">Name of the provider.</param>
        /// <param name="queueCount">How many queues should be load balanced with.</param>
        /// <param name="retry">How long should we wait until the messages is not Completed so it can be put back on the Main queue.</param>
        /// <param name="poison">How long should we wait until the messages is not Completed so it can be put back on the Poison queue.</param>
        /// <param name="messagesForTests">All contracts that are sent in streams.</param>
        /// <returns>SiloBuilder registered.</returns>
        [Obsolete("This method is for use with TestCluster only. Use `pragma warning disable CS0618` to use it without warnings.")]
        public static ISiloBuilder AddGrainsStreamsForTests(this ISiloBuilder builder, string name, int queueCount, TimeSpan retry, TimeSpan poison, params Type[] messagesForTests)
        {
            return builder.AddGrainsStreams(name, queueCount, false, retry, poison, messagesForTests);
        }

        private static ISiloBuilder AddGrainsStreams(this ISiloBuilder builder, string name, int queueCount, bool fireAndForgetDelivery, TimeSpan retry, TimeSpan poison, params Type[] messagesForTests)
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
                              options.RetryTimeout = retry;
                              options.PoisonTimeout = poison;
                              options.FireAndForgetDelivery = fireAndForgetDelivery;
                          })
                          .AddPersistentStreams(name, GrainsQueueAdapterFactory.Create, config =>
                          {
                              config.Configure<GrainsOptions>(options =>
                              {
                                  options.Configure(x => x.RetryTimeout = retry);
                                  options.Configure(x => x.PoisonTimeout = poison);
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