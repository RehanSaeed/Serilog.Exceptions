``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 1 [1607, Anniversary Update] (10.0.14393.1944)
Intel Core i5-6300U CPU 2.40GHz (Skylake), 1 CPU, 4 logical cores and 2 physical cores
Frequency=2437499 Hz, Resolution=410.2566 ns, Timer=TSC
.NET Core SDK=2.1.4
  [Host] : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT
  Clr    : .NET Framework 4.6.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2117.0
  Core   : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT


```
|                 Method |  Job | Runtime |       Mean |      Error |    StdDev |     Median |   Gen 0 | Allocated |
|----------------------- |----- |-------- |-----------:|-----------:|----------:|-----------:|--------:|----------:|
| ReflectionDestructurer |  Clr |     Clr |  14.191 us |  0.2922 us | 0.7799 us |  13.855 us |  3.2043 |   4.96 KB |
|     CustomDestructurer |  Clr |     Clr |   6.831 us |  0.1255 us | 0.1174 us |   6.792 us |  2.2354 |   3.44 KB |
| ReflectionDestructurer | Core |    Core | 775.837 us | 10.0806 us | 7.8702 us | 778.617 us | 10.7422 |   17.2 KB |
|     CustomDestructurer | Core |    Core | 766.221 us | 10.8293 us | 9.0430 us | 766.683 us |  9.7656 |  15.74 KB |
