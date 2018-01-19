using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using Serilog.Exceptions.Destructurers;

namespace Serilog.Exceptions.Benchmark
{
    [ClrJob]
    [CoreJob]
    public class DestructuringBenchmark
    {
        private BenchmarkException benchmarkException;
        private ReflectionBasedDestructurer reflectionBasedDestructurer = new ReflectionBasedDestructurer();
        private BenchmarkExceptionDestructurer benchmarkExceptionDestructurer = new BenchmarkExceptionDestructurer();

        [GlobalSetup]
        public void Setup()
        {
            try
            {
                throw new BenchmarkException()
                {
                    ParamInt = 123,
                    ParamString = "some param value",
                    Point = new Point() {X = 666, Y = 777}
                };
            }
            catch (BenchmarkException ex)
            {
                this.benchmarkException = ex;
            }

        }

        public IReadOnlyDictionary<string, object> DestructureUsingReflectionDestructurer(Exception ex)
        {
            ExceptionPropertiesBag bag = new ExceptionPropertiesBag(ex);

            this.reflectionBasedDestructurer.Destructure(
                ex,
                bag,
                null);

            return bag.GetResultDictionary();
        }

        [Benchmark]
        public IReadOnlyDictionary<string, object> ReflectionDestructurer()
        {
            return DestructureUsingReflectionDestructurer(this.benchmarkException);
        }

        public IReadOnlyDictionary<string, object> DestructureUsingCustomDestructurer(Exception ex)
        {
            ExceptionPropertiesBag bag = new ExceptionPropertiesBag(ex);

            this.benchmarkExceptionDestructurer.Destructure(
                ex,
                bag,
                null);

            return bag.GetResultDictionary();
        }

        [Benchmark]
        public IReadOnlyDictionary<string, object> CustomDestructurer()
        {
            return DestructureUsingCustomDestructurer(this.benchmarkException);
        }
    }
}