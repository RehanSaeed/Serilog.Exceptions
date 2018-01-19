``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 1 [1607, Anniversary Update] (10.0.14393.1944)
Intel Core i5-6300U CPU 2.40GHz (Skylake), 1 CPU, 4 logical cores and 2 physical cores
Frequency=2437499 Hz, Resolution=410.2566 ns, Timer=TSC
.NET Core SDK=2.1.4
  [Host] : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT
  Clr    : .NET Framework 4.6.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2117.0
  Core   : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT


```
|                     Method |  Job | Runtime |       Mean |      Error |     StdDev |
|--------------------------- |----- |-------- |-----------:|-----------:|-----------:|
|  OldReflectionDestructurer |  Clr |     Clr |  15.298 us |  0.1815 us |  0.1516 us |
|         CustomDestructurer |  Clr |     Clr |   6.867 us |  0.0859 us |  0.0804 us |
| FastReflectionDestructurer |  Clr |     Clr |  11.652 us |  0.2179 us |  0.2140 us |
|  OldReflectionDestructurer | Core |    Core | 775.141 us | 10.8932 us | 10.1895 us |
|         CustomDestructurer | Core |    Core | 748.416 us |  4.2248 us |  3.9518 us |
| FastReflectionDestructurer | Core |    Core | 767.221 us | 10.5975 us |  9.9130 us |
