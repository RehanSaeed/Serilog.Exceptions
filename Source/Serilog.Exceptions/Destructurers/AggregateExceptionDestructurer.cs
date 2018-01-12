namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AggregateExceptionDestructurer : ExceptionDestructurer
    {
        public override Type[] TargetTypes => new[] { typeof(AggregateException) };

        public override void Destructure(
            Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException)
        {
            base.Destructure(exception, propertiesBag, destructureException);

            var aggregateException = (AggregateException)exception;

            propertiesBag.AddProperty(
                nameof(AggregateException.InnerExceptions),
                aggregateException.InnerExceptions.Select(destructureException).ToList());
        }
    }
}
