``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 1 [1607, Anniversary Update] (10.0.14393.1944)
Intel Core i5-6300U CPU 2.40GHz (Skylake), 1 CPU, 4 logical cores and 2 physical cores
Frequency=2437499 Hz, Resolution=410.2566 ns, Timer=TSC
.NET Core SDK=2.1.4
  [Host] : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT
  Clr    : .NET Framework 4.6.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2117.0
  Core   : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT


```
|                     Method |  Job | Runtime |       Mean |      Error |      StdDev |     Median |
|--------------------------- |----- |-------- |-----------:|-----------:|------------:|-----------:|
|  OldReflectionDestructurer |  Clr |     Clr |  15.360 us |  0.2928 us |   0.3133 us |  15.412 us |
|         CustomDestructurer |  Clr |     Clr |   6.643 us |  0.1304 us |   0.1395 us |   6.615 us |
| FastReflectionDestructurer |  Clr |     Clr |  11.316 us |  0.2241 us |   0.2201 us |  11.364 us |
|  OldReflectionDestructurer | Core |    Core | 807.670 us | 20.8714 us |  51.1980 us | 792.939 us |
|         CustomDestructurer | Core |    Core | 785.605 us | 10.6901 us |   9.9995 us | 788.961 us |
| FastReflectionDestructurer | Core |    Core | 883.007 us | 42.0822 us | 124.0805 us | 813.333 us |
