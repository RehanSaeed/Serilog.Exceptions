namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AggregateExceptionDestructurer : ExceptionDestructurer
    {
        public AggregateExceptionDestructurer(List<string> ignoredProperties)
            : base(ignoredProperties)
        {
        }

        public override Type[] TargetTypes
        {
            get { return new Type[] { typeof(AggregateException) }; }
        }

        public override void Destructure(
            Exception exception,
            IDictionary<string, object> data,
            Func<Exception, IDictionary<string, object>> destructureException)
        {
            base.Destructure(exception, data, destructureException);

            var aggregateException = (AggregateException)exception;

            data.AddIfNotIgnored(
                nameof(AggregateException.InnerExceptions),
                aggregateException.InnerExceptions.Select(destructureException).ToList(),
                this.IgnoredProperties);
        }
    }
}
