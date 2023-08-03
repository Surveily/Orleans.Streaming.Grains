# Orleans.Streaming.Grains

[![NuGet](https://img.shields.io/nuget/v/Orleans.Streaming.Grains.svg?style=flat)](https://www.nuget.org/packages/Orleans.Streaming.Grains)

This is a Streaming Provider that uses Orleans' built-in `StatefulGrain` system for queuing messages. The extra added value of this project is the `FireAndForgetDelivery` option which can be disabled for tests, so they are easier to write with `TestCluster`.

## Usage

In production you should register the provider using the extension method for `ISiloBuilder`. It is also required to add Grains storage providers for actual state and for subscriptions. The example below uses `MemoryGrainStorage` which should not be used if you require the stream queues to be persistent.

```
siloBuilder.AddMemoryGrainStorageAsDefault()
           .AddMemoryGrainStorage(name: "PubSubStore")
           .AddGrainsStreams(name: "Default", queueCount: 1, retry: 3);
```

In test you should register the provider using the extension method for `ISiloBuilder`. It is also required to add Grains storage providers for actual state and for subscriptions. The example below uses `MemoryGrainStorage` which should not be used if you require the stream queues to be persistent. Additionally you need to provide types for all stream messages and how many queues we want to have per each message. In this situation, every `OnNextAsync` invocation can be awaited and the code will wait until that message is accepted by all subscribers.

```
siloBuilder.ConfigureServices(Configure)
           .AddMemoryGrainStorageAsDefault()
           .AddMemoryGrainStorage(name: "PubSubStore")
           .AddGrainsStreamsForTests(name: "Default", queueCount: 3, retry: 3, new[]
           {
             typeof(BlobMessage),
             typeof(SimpleMessage),
             typeof(CompoundMessage),
             typeof(ExplosiveMessage),
             typeof(BroadcastMessage),
             typeof(ExplosiveNextMessage),
           });
```

## Project Contents:

* **Orleans.Streaming.Grains** implements the Grain Stream Provider.
* **Orleans.Streaming.Grains.Test** test the Grain Stream Provider in an Orleans TestingHost.

*Make sure to open the project folder in VS Code with Remote - Containers extension enabled.*