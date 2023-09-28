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

| Method         | Mean       | Error    | StdDev   | Completed Work Items | Lock Contentions | Allocated |
|--------------- |-----------:|---------:|---------:|---------------------:|-----------------:|----------:|
| BroadcastAsync |   164.6 ms |  4.94 ms | 14.57 ms |             718.3333 |           2.0000 |    1.1 MB |
| CompoundAsync  |   397.9 ms | 20.62 ms | 60.79 ms |            2278.0000 |           5.0000 |   3.41 MB |
| ExplosiveAsync | 1,086.1 ms | 28.81 ms | 83.12 ms |           22663.0000 |          46.0000 |  37.27 MB |