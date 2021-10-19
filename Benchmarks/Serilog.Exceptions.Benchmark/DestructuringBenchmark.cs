namespace Serilog.Exceptions.Benchmark
{
    using System;
    using System.Collections.Generic;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Jobs;
    using Serilog.Exceptions.Destructurers;

    [KeepBenchmarkFiles]
    [MemoryDiagnoser]
    [MinColumn]
    [MaxColumn]
    [HtmlExporter]
    [CsvMeasurementsExporter]
    [RPlotExporter]
    [SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net472)]
    public class DestructuringBenchmark
    {
        private readonly ReflectionBasedDestructurer reflectionBasedDestructurer = new(10);
        private readonly BenchmarkExceptionDestructurer benchmarkExceptionDestructurer = new();
        private BenchmarkException benchmarkException = default!;

        [GlobalSetup]
        public void Setup()
        {
            try
            {
                throw new BenchmarkException()
                {
                    ParamInt = 123,
                    ParamString = "some param value",
                    Point = new Point() { X = 666, Y = 777 },
                };
            }
            catch (BenchmarkException ex)
            {
                this.benchmarkException = ex;
            }
        }

        public IReadOnlyDictionary<string, object?> DestructureUsingReflectionDestructurer(Exception ex)
        {
            var bag = new ExceptionPropertiesBag(ex);

            this.reflectionBasedDestructurer.Destructure(
                ex,
                bag,
                _ => new Dictionary<string, object?>());

            return bag.GetResultDictionary();
        }

        [Benchmark]
        public IReadOnlyDictionary<string, object?> ReflectionDestructurer() =>
            this.DestructureUsingReflectionDestructurer(this.benchmarkException);

        public IReadOnlyDictionary<string, object?> DestructureUsingCustomDestructurer(Exception ex)
        {
            var bag = new ExceptionPropertiesBag(ex);

            this.benchmarkExceptionDestructurer.Destructure(
                ex,
                bag,
                _ => new Dictionary<string, object?>());

            return bag.GetResultDictionary();
        }

        [Benchmark]
        public IReadOnlyDictionary<string, object?> CustomDestructurer() =>
            this.DestructureUsingCustomDestructurer(this.benchmarkException);
    }
}
