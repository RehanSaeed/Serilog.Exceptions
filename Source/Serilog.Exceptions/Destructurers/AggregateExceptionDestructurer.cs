namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Serilog.Exceptions.Core;

    /// <summary>
    /// Destructurer for <see cref="AggregateException"/>.
    /// </summary>
    public class AggregateExceptionDestructurer : ExceptionDestructurer
    {
        /// <inheritdoc cref="IExceptionDestructurer.TargetTypes"/>
        public override Type[] TargetTypes => new[] { typeof(AggregateException) };

        /// <inheritdoc cref="IExceptionDestructurer.Destructure"/>
        public override void Destructure(
            Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException)
        {
            base.Destructure(exception, propertiesBag, destructureException);

#pragma warning disable CA1062 // Validate arguments of public methods
            var aggregateException = (AggregateException)exception;
            propertiesBag.AddProperty(
                nameof(AggregateException.InnerExceptions),
                aggregateException.InnerExceptions.Select(destructureException).ToList());
#pragma warning restore CA1062 // Validate arguments of public methods
        }
    }
}
