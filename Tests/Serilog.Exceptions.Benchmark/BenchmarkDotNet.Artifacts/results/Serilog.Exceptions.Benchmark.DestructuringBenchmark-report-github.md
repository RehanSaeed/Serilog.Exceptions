``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 1 [1607, Anniversary Update] (10.0.14393.1944)
Intel Core i5-6300U CPU 2.40GHz (Skylake), 1 CPU, 4 logical cores and 2 physical cores
Frequency=2437504 Hz, Resolution=410.2557 ns, Timer=TSC
.NET Core SDK=2.1.4
  [Host] : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT
  Clr    : .NET Framework 4.6.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2117.0
  Core   : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT


```
|                     Method |  Job | Runtime |       Mean |      Error |     StdDev |     Median |   Gen 0 | Allocated |
|--------------------------- |----- |-------- |-----------:|-----------:|-----------:|-----------:|--------:|----------:|
| FastReflectionDestructurer |  Clr |     Clr |  11.555 us |  0.2609 us |  0.4359 us |  11.400 us |  3.2196 |   4.96 KB |
|     ReflectionDestructurer |  Clr |     Clr |  14.164 us |  0.2820 us |  0.8182 us |  13.810 us |  3.2196 |   4.96 KB |
|         CustomDestructurer |  Clr |     Clr |   6.585 us |  0.0813 us |  0.0721 us |   6.565 us |  2.2354 |   3.44 KB |
| FastReflectionDestructurer | Core |    Core | 756.909 us | 11.7003 us |  9.7703 us | 755.749 us | 10.7422 |   17.2 KB |
|     ReflectionDestructurer | Core |    Core | 758.189 us | 13.6420 us | 12.0933 us | 753.921 us | 10.7422 |   17.2 KB |
|         CustomDestructurer | Core |    Core | 740.791 us | 13.1880 us | 12.3361 us | 741.054 us |  9.7656 |  15.74 KB |
