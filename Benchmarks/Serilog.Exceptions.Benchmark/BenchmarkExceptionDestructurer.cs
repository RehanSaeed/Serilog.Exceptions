namespace Serilog.Exceptions.Benchmark
{
    using System;
    using System.Collections.Generic;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Destructurers;

    /// <summary>
    /// A destructurer used in benchmarks.
    /// </summary>
    /// <seealso cref="ExceptionDestructurer" />
    public class BenchmarkExceptionDestructurer : ExceptionDestructurer
    {
        /// <inheritdoc />
        public override Type[] TargetTypes => new[] { typeof(BenchmarkException) };

        /// <inheritdoc />
        public override void Destructure(
            Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException)
        {
            base.Destructure(exception, propertiesBag, destructureException);

#pragma warning disable CA1062 // Validate arguments of public methods
            var benchmarkException = (BenchmarkException)exception;
            propertiesBag.AddProperty("ParamString", benchmarkException.ParamString);
            propertiesBag.AddProperty("ParamInt", benchmarkException.ParamInt);
            propertiesBag.AddProperty("Point", new Dictionary<string, object>
            {
                { "X", benchmarkException.Point.X },
                { "Y", benchmarkException.Point.Y },
            });
#pragma warning restore CA1062 // Validate arguments of public methods
        }
    }
}
