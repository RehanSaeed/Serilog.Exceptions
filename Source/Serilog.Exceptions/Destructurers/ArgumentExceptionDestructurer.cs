namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;

    public class ArgumentExceptionDestructurer : ExceptionDestructurer
    {
        public override Type[] TargetTypes => new[]
        {
            typeof(ArgumentException),
            typeof(ArgumentNullException)
        };

        public override void Destructure(
            Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException)
        {
            base.Destructure(exception, propertiesBag, destructureException);

            var argumentException = (ArgumentException)exception;

            propertiesBag.AddProperty(nameof(ArgumentException.ParamName), argumentException.ParamName);
        }
    }
}