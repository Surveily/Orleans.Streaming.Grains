// * Summary *

BenchmarkDotNet v0.13.8, Debian GNU/Linux 11 (bullseye) (container)
AMD Ryzen 7 5800HS with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.401
  [Host]     : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2 [AttachedDebugger]
  Job-KBMLFL : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2

BASIC

Concurrent=True  Server=False  

| Method         | Mean       | Error    | StdDev    | Gen0     | Completed Work Items | Lock Contentions | Gen1    | Allocated |
|--------------- |-----------:|---------:|----------:|---------:|---------------------:|-----------------:|--------:|----------:|
| BroadcastAsync |   298.0 us | 12.21 us |  34.83 us |  12.2070 |              87.0396 |           0.3350 |  3.9063 | 103.99 KB |
| CompoundAsync  |   491.2 us | 37.30 us | 103.36 us |  21.4844 |              44.0693 |           0.8809 |  7.8125 | 179.77 KB |
| ExplosiveAsync | 1,098.3 us | 54.18 us | 158.05 us | 111.3281 |             198.2754 |           0.4297 | 33.2031 | 926.87 KB |

PERSIST

Concurrent=True  Server=False  

| Method         | Mean       | Error     | StdDev      | Median     | Gen0       | Completed Work Items | Lock Contentions | Gen1       | Allocated    |
|--------------- |-----------:|----------:|------------:|-----------:|-----------:|---------------------:|-----------------:|-----------:|-------------:|
| BroadcastAsync |   154.8 ms |   6.05 ms |    17.83 ms |   156.4 ms |          - |             307.7500 |                - |          - |    530.46 KB |
| CompoundAsync  |   431.0 ms |  19.59 ms |    57.77 ms |   446.4 ms |          - |            1008.0000 |           1.0000 |          - |   1685.62 KB |
| ExplosiveAsync | 2,686.8 ms | 341.64 ms | 1,007.34 ms | 3,218.6 ms | 77000.0000 |          833967.0000 |          81.0000 | 12000.0000 | 663634.86 KB |

Concurrent=True  Server=True  

| Method         | Mean       | Error     | StdDev      | Median     | Completed Work Items | Lock Contentions | Allocated   |
|--------------- |-----------:|----------:|------------:|-----------:|---------------------:|-----------------:|------------:|
| BroadcastAsync |   149.7 ms |   5.67 ms |    16.71 ms |   149.9 ms |             301.2500 |                - |   522.99 KB |
| CompoundAsync  |   403.4 ms |  23.31 ms |    68.73 ms |   405.2 ms |             943.0000 |                - |  1476.31 KB |
| ExplosiveAsync | 2,243.9 ms | 381.31 ms | 1,124.31 ms | 3,065.3 ms |           14948.0000 |          44.0000 | 67099.35 KB |