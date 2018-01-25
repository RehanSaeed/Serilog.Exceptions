using BenchmarkDotNet.Running;

namespace Serilog.Exceptions.Benchmark
{
    public class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<DestructuringBenchmark>();
        }
    }
}
