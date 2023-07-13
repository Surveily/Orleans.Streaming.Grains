// <copyright file="BaseTest.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Orleans.Streaming.Grains.Test
{
    public abstract class BaseTest<T>
        where T : class
    {
#pragma warning disable CS8618
        public BaseTest()
        {
            Services = new ServiceCollection();
        }
#pragma warning restore CS8618

        public T Subject { get; private set; }

        public ServiceCollection Services { get; }

        [OneTimeSetUp]
        public virtual Task SetupAsync()
        {
            if (Services.All(x => x.ServiceType != typeof(T)))
            {
                Services.AddTransient<T>();
            }

            var provider = Services.BuildServiceProvider();

            if (provider != null)
            {
                var service = provider.GetService<T>();

                if (service != null)
                {
                    Subject = service;
                }
                else
                {
                    throw new InvalidOperationException("Subject not registered.");
                }
            }

            return Task.CompletedTask;
        }
    }
}