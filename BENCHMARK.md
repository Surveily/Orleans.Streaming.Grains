## Summary

BenchmarkDotNet v0.13.8, Debian GNU/Linux 11 (bullseye) (container)

Intel Xeon CPU E5-2673 v3 2.40GHz, 1 CPU, 2 logical and 2 physical cores

.NET SDK 7.0.401

## MemoryStreams

Concurrent=True  Server=True  

| Method         | Mean     | Error    | StdDev   | Median   | Completed Work Items | Lock Contentions | Allocated  |
|--------------- |---------:|---------:|---------:|---------:|---------------------:|-----------------:|-----------:|
| BroadcastAsync | 100.8 ms |  0.91 ms |  0.71 ms | 100.9 ms |              77.4000 |           0.8000 |  186.75 KB |
| CompoundAsync  | 192.9 ms |  5.97 ms | 16.13 ms | 200.3 ms |             176.0000 |           0.6667 |   306.7 KB |
| ExplosiveAsync | 358.2 ms | 18.27 ms | 53.87 ms | 397.9 ms |             420.0000 |           1.0000 | 1034.32 KB |

## GrainStreams

Concurrent=True  Server=True  

| Method         | Mean       | Error    | StdDev   | Completed Work Items | Lock Contentions | Allocated |
|--------------- |-----------:|---------:|---------:|---------------------:|-----------------:|----------:|
| BroadcastAsync |   166.0 ms |  5.70 ms | 16.81 ms |             912.0000 |           2.2500 |   1.45 MB |
| CompoundAsync  |   466.2 ms | 23.31 ms | 68.74 ms |            1988.0000 |           1.0000 |    3.1 MB |
| ExplosiveAsync | 1,102.5 ms | 30.21 ms | 87.65 ms |            9045.0000 |          15.0000 |  13.89 MB |