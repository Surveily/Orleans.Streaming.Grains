## Summary

BenchmarkDotNet v0.13.8, Debian GNU/Linux 11 (bullseye) (container)

Intel Xeon CPU E5-2673 v3 2.40GHz, 1 CPU, 2 logical and 2 physical cores

.NET SDK 7.0.401

## MemoryStreams

Concurrent=True  Server=True  

| Method         | Mean      | Error     | StdDev   | Median    | Completed Work Items | Lock Contentions | Allocated  |
|--------------- |----------:|----------:|---------:|----------:|---------------------:|-----------------:|-----------:|
| BroadcastAsync |  77.87 ms |  4.684 ms | 13.81 ms |  81.58 ms |              83.1429 |           0.4286 |  155.37 KB |
| CompoundAsync  | 184.10 ms |  5.029 ms | 14.83 ms | 193.41 ms |             187.2500 |                - |  301.05 KB |
| ExplosiveAsync | 356.11 ms | 10.680 ms | 31.49 ms | 344.16 ms |             459.5000 |           2.0000 | 1563.35 KB |

## GrainStreams

Concurrent=True  Server=True  

| Method         | Mean      | Error    | StdDev    | Completed Work Items | Lock Contentions | Allocated  |
|--------------- |----------:|---------:|----------:|---------------------:|-----------------:|-----------:|
| BroadcastAsync |  79.85 ms | 3.576 ms | 10.544 ms |             114.1250 |           0.6250 |  227.72 KB |
| CompoundAsync  | 193.46 ms | 3.821 ms |  7.976 ms |             356.3333 |           2.3333 |  593.62 KB |
| ExplosiveAsync | 316.61 ms | 9.694 ms | 28.278 ms |            2292.5000 |           9.0000 | 3542.52 KB |