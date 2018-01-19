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
        private ReflectionBasedDestructurer oldReflectionBasedDestructurer = new ReflectionBasedDestructurer();
        private BenchmarkExceptionDestructurer benchmarkExceptionDestructurer = new BenchmarkExceptionDestructurer();
        private FastReflectionBasedDestructurer fastReflectionBasedDestructurer = new FastReflectionBasedDestructurer();

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

        public IReadOnlyDictionary<string, object> DestructureUsingOldReflectionDestructurer(Exception ex)
        {
            ExceptionPropertiesBag bag = new ExceptionPropertiesBag(ex);

            this.oldReflectionBasedDestructurer.Destructure(
                ex,
                bag,
                null);

            return bag.GetResultDictionary();
        }

        [Benchmark]
        public IReadOnlyDictionary<string, object> OldReflectionDestructurer()
        {
            return DestructureUsingOldReflectionDestructurer(this.benchmarkException);
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

        public IReadOnlyDictionary<string, object> DestructureUsingFastReflectionDestructurer(Exception ex)
        {
            ExceptionPropertiesBag bag = new ExceptionPropertiesBag(ex);

            this.fastReflectionBasedDestructurer.Destructure(
                ex,
                bag,
                null);

            return bag.GetResultDictionary();
        }

        [Benchmark]
        public IReadOnlyDictionary<string, object> FastReflectionDestructurer()
        {
            return DestructureUsingFastReflectionDestructurer(this.benchmarkException);
        }
    }
}