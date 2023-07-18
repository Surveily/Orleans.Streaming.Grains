// <copyright file="OneToMany.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Streaming.Grains.Test;

namespace Orleans.Streaming.Grains.Tests.Streams.Scenarios
{
    public class OneToMany
    {
        public class Config : BaseGrainTestConfig, IDisposable
        {
            private bool _isDisposed;

            public override void Configure(IServiceCollection services)
            {
                /* dependency injection code here */
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
}