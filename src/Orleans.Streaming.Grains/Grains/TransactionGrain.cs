// <copyright file="TransactionGrain.cs" company="Surveily Sp. z o.o.">
// Copyright (c) Surveily Sp. z o.o.. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streaming.Grains.Abstract;
using Orleans.Streaming.Grains.State;
using Orleans.Streaming.Grains.Streams;
using Orleans.Utilities;

namespace Orleans.Streaming.Grains.Grains
{
    public class TransactionGrain : Grain<TransactionGrainState>, ITransactionGrain
    {
        private readonly GrainsOptions _options;

        public TransactionGrain(GrainsOptions options)
        {
            _options = options;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if (State.Queue == null)
            {
                State.Queue = new Queue<Guid>();
                State.Poison = new Queue<Guid>();
                State.Transactions = new Dictionary<Guid, DateTimeOffset>();

                await WriteStateAsync();
            }

            var timeout = _options.Timeout / 5;

            _ = RegisterTimer(FlushAsync, null, timeout, timeout);

            await base.OnActivateAsync(cancellationToken);
        }

        public async Task CompleteAsync(Guid id, bool success)
        {
            if (State.Transactions.Remove(id, out _))
            {
                if (!success)
                {
                    State.Poison.Enqueue(id);
                }

                await WriteStateAsync();
            }
        }

        public async Task<Guid?> PopAsync()
        {
            if (State.Queue.TryDequeue(out var id))
            {
                State.Transactions.Add(id, DateTimeOffset.UtcNow);

                await WriteStateAsync();

                return id;
            }

            return null;
        }

        public async Task PostAsync(Guid id)
        {
            State.Queue.Enqueue(id);

            await WriteStateAsync();
        }

        public Task<(Queue<Guid> Queue, Queue<Guid> Poison, Dictionary<Guid, DateTimeOffset> Transactions)> GetStateAsync()
        {
            return Task.FromResult((State.Queue, State.Poison, State.Transactions));
        }

        private async Task FlushAsync(object arg)
        {
            var expired = State.Transactions.Where(x => (DateTimeOffset.UtcNow - x.Value) > _options.Timeout)
                                            .ToList();

            if (expired.Any())
            {
                foreach (var item in expired)
                {
                    State.Queue.Enqueue(item.Key);
                    State.Transactions.Remove(item.Key);
                }

                await WriteStateAsync();
            }
        }
    }
}