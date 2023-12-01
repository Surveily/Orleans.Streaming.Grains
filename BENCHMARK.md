## Summary

BenchmarkDotNet v0.13.10, Debian GNU/Linux 12 (bookworm) (container)
AMD EPYC 7763, 1 CPU, 4 logical and 2 physical cores
.NET SDK 7.0.404
  [Host]     : .NET 7.0.14 (7.0.1423.51910), X64 RyuJIT AVX2
  Job-WBNRVE : .NET 7.0.14 (7.0.1423.51910), X64 RyuJIT AVX2

Concurrent=True  Server=True  

## BroadcastChannels

| Method         | Mean     | Error     | StdDev    | Completed Work Items | Lock Contentions | Allocated |
|--------------- |---------:|----------:|----------:|---------------------:|-----------------:|----------:|
| BroadcastAsync |       NA |        NA |        NA |                   NA |               NA |        NA |
| CompoundAsync  | 8.124 ms | 0.1347 ms | 0.1260 ms |              23.4688 |           0.0469 |  81.43 KB |
| ExplosiveAsync | 8.128 ms | 0.1193 ms | 0.1116 ms |              93.2813 |           0.7969 | 236.27 KB |

## MemoryStreams

| Method         | Mean      | Error     | StdDev   | Median    | Completed Work Items | Lock Contentions | Allocated |
|--------------- |----------:|----------:|---------:|----------:|---------------------:|-----------------:|----------:|
| BroadcastAsync |  75.41 ms |  4.078 ms | 12.02 ms |  76.56 ms |              67.4286 |           0.1429 | 146.83 KB |
| CompoundAsync  | 184.01 ms |  5.316 ms | 15.68 ms | 191.90 ms |             211.0000 |           0.3333 | 374.21 KB |
| ExplosiveAsync | 354.95 ms | 11.381 ms | 33.56 ms | 343.86 ms |             512.0000 |           1.0000 | 1257.7 KB |

## GrainStreams

| Method         | Mean      | Error     | StdDev   | Completed Work Items | Lock Contentions | Allocated  |
|--------------- |----------:|----------:|---------:|---------------------:|-----------------:|-----------:|
| BroadcastAsync |  67.44 ms |  4.597 ms | 13.56 ms |             166.1667 |           0.6667 |  316.36 KB |
| CompoundAsync  | 149.63 ms |  6.787 ms | 20.01 ms |             383.5000 |           0.5000 |   748.1 KB |
| ExplosiveAsync | 327.40 ms | 11.230 ms | 33.11 ms |            2685.0000 |          10.0000 | 4231.54 KB |