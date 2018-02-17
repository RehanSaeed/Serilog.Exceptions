``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 1 [1607, Anniversary Update] (10.0.14393.1944)
Intel Core i5-6300U CPU 2.40GHz (Skylake), 1 CPU, 4 logical cores and 2 physical cores
Frequency=2437506 Hz, Resolution=410.2554 ns, Timer=TSC
.NET Core SDK=2.1.4
  [Host] : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT
  Clr    : .NET Framework 4.6.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2117.0
  Core   : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT


```
|                 Method |  Job | Runtime |       Mean |      Error |     StdDev |  Gen 0 | Allocated |
|----------------------- |----- |-------- |-----------:|-----------:|-----------:|-------:|----------:|
| ReflectionDestructurer |  Clr |     Clr |   9.966 us |  0.1979 us |  0.3196 us | 2.4719 |   3.81 KB |
|     CustomDestructurer |  Clr |     Clr |   7.567 us |  0.0839 us |  0.0701 us | 2.2278 |   3.44 KB |
| ReflectionDestructurer | Core |    Core | 850.053 us |  7.5538 us |  7.0658 us | 9.7656 |  16.11 KB |
|     CustomDestructurer | Core |    Core | 868.570 us | 17.1272 us | 17.5884 us | 9.7656 |  15.74 KB |
