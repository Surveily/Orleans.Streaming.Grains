# Why?

This library allows you to use [Grains Storage](https://learn.microsoft.com/en-us/dotnet/orleans/grains/grain-persistence/?pivots=orleans-7-0) as a storage layer for Persistent Streams. Our benchmarks show 10% speed advantage over `MemoryStreams`.

# Orleans.Streaming.Grains

[![NuGet](https://img.shields.io/nuget/v/Orleans.Streaming.Grains.svg?style=flat)](https://www.nuget.org/packages/Orleans.Streaming.Grains)

This is a Streaming Provider that uses Orleans' built-in `StatefulGrain` system for queuing messages. The extra added value of this project is the `FireAndForgetDelivery` option which can be disabled for tests, so they are easier to write with `TestCluster`.

## Usage

In production you should register the provider using the extension method for `ISiloBuilder`. It is also required to add Grains storage providers for actual state and for subscriptions. The example below uses `MemoryGrainStorage` which should not be used if you require the stream queues to be persistent.

```c#
siloBuilder.AddMemoryGrainStorageAsDefault()
           .AddMemoryGrainStorage(name: "PubSubStore")
           .AddGrainsStreams(name: "Default",
                             queueCount: 1,
                             retry: TimeSpan.FromMinutes(1),
                             poison: TimeSpan.FromMinutes(3));
```

In test you should register the provider using the extension method for `ISiloBuilder`. It is also required to add Grains storage providers for actual state and for subscriptions. The example below uses `MemoryGrainStorage` which should not be used if you require the stream queues to be persistent. In this situation, every `OnNextAsync` invocation can be awaited and the code will wait until that message is accepted by all subscribers.

```c#
siloBuilder.ConfigureServices(Configure)
           .AddMemoryGrainStorageAsDefault()
           .AddMemoryGrainStorage(name: "PubSubStore")
           .AddGrainsStreamsForTests(name: "Default",
                                     queueCount: 3,
                                     retry: TimeSpan.FromSeconds(1),
                                     poison: TimeSpan.FromSeconds(3));
```

## Compromise

In high throughput and high volume environments you will want to collect the `TransactionItemGrain` objects early so they don't overload the memory of your silo. Use the below configuration and adjust it to your requirements:

```c#
siloBuilder.Configure<GrainCollectionOptions>(options =>
{
    options.CollectionAge = TimeSpan.FromMinutes(1);
    options.CollectionQuantum = TimeSpan.FromSeconds(5);
    options.ClassSpecificCollectionAge[typeof(TransactionItemGrain<>).FullName] = options.CollectionQuantum * 2;
});
```

## Project Contents:

* **Orleans.Streaming.Grains** implements the Grain Stream Provider.
* **Orleans.Streaming.Grains.Test** test the Grain Stream Provider in an Orleans TestingHost.

*Make sure to open the project folder in VS Code with Remote - Containers extension enabled.*