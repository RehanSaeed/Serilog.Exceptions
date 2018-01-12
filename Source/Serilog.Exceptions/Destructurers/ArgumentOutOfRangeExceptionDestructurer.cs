namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;

    public class ArgumentOutOfRangeExceptionDestructurer : ArgumentExceptionDestructurer
    {
        public override Type[] TargetTypes
        {
            get
            {
                return new Type[]
                {
                    typeof(ArgumentOutOfRangeException)
                };
            }
        }

        public override void Destructure(
            Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException)
        {
            base.Destructure(exception, propertiesBag, destructureException);

            var argumentException = (ArgumentOutOfRangeException)exception;

            propertiesBag.AddProperty(nameof(ArgumentOutOfRangeException.ActualValue), argumentException.ActualValue);
        }
    }
}