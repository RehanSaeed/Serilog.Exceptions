namespace Serilog.Exceptions.Benchmark;

using BenchmarkDotNet.Running;

public static class Program
{
    public static void Main() => BenchmarkRunner.Run<DestructuringBenchmark>();
}
