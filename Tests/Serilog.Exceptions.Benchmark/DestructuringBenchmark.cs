using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using Serilog.Exceptions.Destructurers;
using System;
using System.Collections.Generic;
using Serilog.Exceptions.Core;

namespace Serilog.Exceptions.Benchmark
{
    [ClrJob]
    [CoreJob]
    [MemoryDiagnoser]
    public class DestructuringBenchmark
    {
        private BenchmarkException benchmarkException;
        private ReflectionBasedDestructurer reflectionBasedDestructurer = new ReflectionBasedDestructurer(10);
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

        public IReadOnlyDictionary<string, object> DestructureUsingReflectionDestructurer(Exception ex, IExceptionPropertiesBag exceptionPropertiesBag)
        {
            this.reflectionBasedDestructurer.Destructure(
                ex,
                exceptionPropertiesBag,
                null);

            return exceptionPropertiesBag.GetResultDictionary();
        }

        [Benchmark]
        public IReadOnlyDictionary<string, object> ReflectionDestructurer()
        {
            return DestructureUsingReflectionDestructurer(this.benchmarkException, new ExceptionPropertiesBag(this.benchmarkException));
        }

        public IReadOnlyDictionary<string, object> DestructureUsingCustomDestructurer(Exception ex, IExceptionPropertiesBag exceptionPropertiesBag)
        {
            this.benchmarkExceptionDestructurer.Destructure(
                ex,
                exceptionPropertiesBag,
                null);

            return exceptionPropertiesBag.GetResultDictionary();
        }

        [Benchmark]
        public IReadOnlyDictionary<string, object> CustomDestructurer()
        {
            return DestructureUsingCustomDestructurer(this.benchmarkException, new ExceptionPropertiesBag(this.benchmarkException));
        }
    }
}