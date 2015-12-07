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
            IDictionary<string, object> data,
            Func<Exception, IDictionary<string, object>> destructureException)
        {
            base.Destructure(exception, data, destructureException);

            var argumentException = (ArgumentOutOfRangeException)exception;

            data.Add(nameof(ArgumentOutOfRangeException.ActualValue), argumentException.ActualValue);
        }
    }
}