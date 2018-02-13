``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 1 [1607, Anniversary Update] (10.0.14393.1944)
Intel Core i5-6300U CPU 2.40GHz (Skylake), 1 CPU, 4 logical cores and 2 physical cores
Frequency=2437504 Hz, Resolution=410.2557 ns, Timer=TSC
.NET Core SDK=2.1.4
  [Host] : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT
  Clr    : .NET Framework 4.6.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2117.0
  Core   : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT


```
|                     Method |  Job | Runtime |       Mean |      Error |     StdDev |   Gen 0 | Allocated |
|--------------------------- |----- |-------- |-----------:|-----------:|-----------:|--------:|----------:|
| FastReflectionDestructurer |  Clr |     Clr |  10.659 us |  0.3873 us |  0.3433 us |  3.2196 |   4.96 KB |
|     ReflectionDestructurer |  Clr |     Clr |  13.532 us |  0.1775 us |  0.1573 us |  3.2196 |   4.96 KB |
|         CustomDestructurer |  Clr |     Clr |   6.726 us |  0.3140 us |  0.3491 us |  2.2354 |   3.44 KB |
| FastReflectionDestructurer | Core |    Core | 729.515 us | 10.8557 us | 10.1544 us | 10.7422 |   17.2 KB |
|     ReflectionDestructurer | Core |    Core | 729.297 us | 10.1203 us |  9.4665 us | 10.7422 |   17.2 KB |
|         CustomDestructurer | Core |    Core | 736.542 us |  9.6608 us |  8.0672 us |  9.7656 |  15.74 KB |
