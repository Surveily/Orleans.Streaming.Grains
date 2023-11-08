## Summary

BenchmarkDotNet v0.13.8, Debian GNU/Linux 11 (bullseye) (container)

Intel Xeon CPU E5-2673 v3 2.40GHz, 1 CPU, 2 logical and 2 physical cores

.NET SDK 7.0.401

## MemoryStreams

Concurrent=True  Server=True  

| Method         | Mean      | Error     | StdDev   | Completed Work Items | Lock Contentions | Allocated  |
|--------------- |----------:|----------:|---------:|---------------------:|-----------------:|-----------:|
| BroadcastAsync |  74.79 ms |  4.108 ms | 12.11 ms |              87.3333 |           2.0000 |  158.92 KB |
| CompoundAsync  | 164.04 ms |  6.374 ms | 18.80 ms |             199.7500 |           3.7500 |  307.64 KB |
| ExplosiveAsync | 272.35 ms | 13.966 ms | 38.47 ms |             550.0000 |           9.0000 | 1100.84 KB |

## GrainStreams

Concurrent=True  Server=True  

| Method         | Mean      | Error     | StdDev   | Completed Work Items | Lock Contentions | Allocated  |
|--------------- |----------:|----------:|---------:|---------------------:|-----------------:|-----------:|
| BroadcastAsync |  72.49 ms |  4.870 ms | 14.36 ms |             147.2857 |           2.5714 |  257.12 KB |
| CompoundAsync  | 157.27 ms |  7.281 ms | 21.47 ms |             341.5000 |           3.7500 |  572.71 KB |
| ExplosiveAsync | 332.09 ms | 12.292 ms | 36.24 ms |            1898.0000 |          11.0000 | 2982.95 KB |