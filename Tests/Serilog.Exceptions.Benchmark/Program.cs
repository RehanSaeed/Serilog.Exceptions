namespace Serilog.Exceptions.Benchmark
{
    using BenchmarkDotNet.Running;

    public class Program
    {
        static void Main() => BenchmarkRunner.Run<DestructuringBenchmark>();
    }
}
