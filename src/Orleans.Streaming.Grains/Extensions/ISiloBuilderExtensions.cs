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
        public static ISiloBuilder AddGrainsStreams(this ISiloBuilder builder, string name, int queueCount)
        {
            return builder.AddGrainsStreams(name, queueCount, true);
        }

        public static ISiloBuilder AddGrainsStreamsForTests(this ISiloBuilder builder, string name, params Type[] messagesForTests)
        {
            return builder.AddGrainsStreams(name, 0, false, messagesForTests);
        }

        private static ISiloBuilder AddGrainsStreams(this ISiloBuilder builder, string name, int queueCount, bool fireAndForgetDelivery, params Type[] messagesForTests)
        {
            return builder.ConfigureServices(services =>
                          {
                              services.AddSingleton<ITransactionService, TransactionService>();

                              if (!fireAndForgetDelivery)
                              {
                                  services.AddSingletonNamedService<IStreamQueueBalancer>(name, (f, n) => new GrainsQueueBalancer());
                                  services.AddSingletonNamedService<IStreamQueueMapper>(name, (f, n) => new GrainsQueueMapper(messagesForTests));
                              }
                          })
                          .AddPersistentStreams(name, GrainsQueueAdapterFactory.Create, config =>
                          {
                              config.Configure<GrainsOptions>(options =>
                              {
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