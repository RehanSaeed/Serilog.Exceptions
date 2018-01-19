``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 1 [1607, Anniversary Update] (10.0.14393.1944)
Intel Core i5-6300U CPU 2.40GHz (Skylake), 1 CPU, 4 logical cores and 2 physical cores
Frequency=2437499 Hz, Resolution=410.2566 ns, Timer=TSC
.NET Core SDK=2.1.4
  [Host] : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT
  Clr    : .NET Framework 4.6.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2117.0
  Core   : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT


```
|                 Method |  Job | Runtime |       Mean |      Error |     StdDev |
|----------------------- |----- |-------- |-----------:|-----------:|-----------:|
| ReflectionDestructurer |  Clr |     Clr |  11.180 us |  0.1812 us |  0.1415 us |
|     CustomDestructurer |  Clr |     Clr |   6.408 us |  0.0514 us |  0.0456 us |
| ReflectionDestructurer | Core |    Core | 742.092 us |  7.2303 us |  6.0376 us |
|     CustomDestructurer | Core |    Core | 750.728 us | 18.9935 us | 34.2492 us |
