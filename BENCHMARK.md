## Summary

BenchmarkDotNet v0.13.8, Debian GNU/Linux 11 (bullseye) (container)

Intel Xeon CPU E5-2673 v3 2.40GHz, 1 CPU, 2 logical and 2 physical cores

.NET SDK 7.0.401

## MemoryStreams

Concurrent=True  Server=True  

| Method         | Mean      | Error     | StdDev    | Median    | Completed Work Items | Lock Contentions | Allocated  |
|--------------- |----------:|----------:|----------:|----------:|---------------------:|-----------------:|-----------:|
| BroadcastAsync |  94.82 ms |  2.148 ms |  6.334 ms |  96.67 ms |             103.7143 |           1.1429 |  167.54 KB |
| CompoundAsync  | 177.99 ms |  6.631 ms | 19.551 ms | 191.30 ms |             181.6667 |           1.3333 |  298.69 KB |
| ExplosiveAsync | 351.57 ms | 17.234 ms | 50.814 ms | 382.60 ms |             502.0000 |           2.0000 | 1096.38 KB |

## GrainStreams

Concurrent=True  Server=True  

| Method         | Mean      | Error     | StdDev   | Completed Work Items | Lock Contentions | Allocated  |
|--------------- |----------:|----------:|---------:|---------------------:|-----------------:|-----------:|
| BroadcastAsync |  78.25 ms |  4.725 ms | 13.93 ms |             129.8333 |           0.8333 |  315.02 KB |
| CompoundAsync  | 144.83 ms |  6.416 ms | 18.92 ms |             288.5000 |           1.2500 |  570.87 KB |
| ExplosiveAsync | 315.64 ms | 11.602 ms | 34.21 ms |            1741.5000 |          10.5000 | 3212.29 KB |