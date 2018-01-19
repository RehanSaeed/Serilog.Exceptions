using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Running;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Destructurers;
using Serilog.Exceptions.Filters;

namespace Serilog.Exceptions.Benchmark
{
    public class Program
    {
        internal class ExceptionPropertiesBag : IExceptionPropertiesBag
        {
            private readonly Exception exception;
            private readonly IExceptionPropertyFilter filter;
            private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

            // We keep a note on whether the results were collected to be sure that
            // after that there are no changes. This is the application of fail-fast principle.
            private bool resultsCollected = false;

            public ExceptionPropertiesBag(Exception exception, IExceptionPropertyFilter filter = null)
            {
                if (exception == null)
                {
                    throw new ArgumentNullException(nameof(exception),
                        $"Cannot create {nameof(ExceptionPropertiesBag)} for null exception");
                }

                this.exception = exception;
                this.filter = filter;
            }

            public IReadOnlyDictionary<string, object> GetResultDictionary()
            {
                this.resultsCollected = true;
                return this.properties;
            }

            public void AddProperty(string key, object value)
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key), "Cannot add exception property without a key");
                }

                if (this.resultsCollected)
                {
                    throw new InvalidOperationException(
                        $"Cannot add exception property '{key}' to bag, after results were already collected");
                }

                if (this.filter != null)
                {
                    if (this.filter.ShouldPropertyBeFiltered(this.exception, key, value))
                    {
                        return;
                    }
                }

                this.properties.Add(key, value);
            }
        }

        public class Point
        {
            public Point()
            {
                this.Z = 3;
            }
            public int X { get; set; }
            public int Y { get; set; }
            private int Z { get; set; }
        }

        public class BenchmarkException : Exception
        {
            public string ParamString { get; set; }
            public int ParamInt { get; set; }
            public Point Point { get; set; }

        }

        public class BenchmarkExceptionDestructurer : ExceptionDestructurer
        {
            public override Type[] TargetTypes => new [] { typeof(BenchmarkException) };
            public override void Destructure(Exception exception, IExceptionPropertiesBag propertiesBag, Func<Exception, IReadOnlyDictionary<string, object>> destructureException)
            {
                base.Destructure(exception, propertiesBag, destructureException);
                BenchmarkException benchmarkException = (BenchmarkException)exception;
                propertiesBag.AddProperty("ParamString", benchmarkException.ParamString);
                propertiesBag.AddProperty("ParamInt", benchmarkException.ParamInt);
                propertiesBag.AddProperty("Point", new Dictionary<string, object>
                {
                    {"X", benchmarkException.Point.X},
                    {"Y", benchmarkException.Point.Y}
                });
            }
        }

        [ClrJob]
        [CoreJob]
        public class DestructuringBenchmark
        {
            private BenchmarkException _benchmarkException;
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
                        Point = new Point() { X = 666, Y = 777 }
                    };
                }
                catch (BenchmarkException ex)
                {
                    this._benchmarkException = ex;
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
                return DestructureUsingOldReflectionDestructurer(this._benchmarkException);
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
                return DestructureUsingCustomDestructurer(this._benchmarkException);
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
                return DestructureUsingFastReflectionDestructurer(this._benchmarkException);
            }
        }

        static void Main()
        {
            BenchmarkRunner.Run<DestructuringBenchmark>();
        }
    }
}
