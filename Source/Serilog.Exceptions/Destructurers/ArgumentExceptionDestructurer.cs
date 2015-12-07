namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;

    public class ArgumentExceptionDestructurer : ExceptionDestructurer
    {
        public override Type[] TargetTypes
        {
            get
            {
                return new Type[] 
                {
                    typeof(ArgumentException),
                    typeof(ArgumentNullException)
                };
            }
        }

        public override void Destructure(
            Exception exception,
            IDictionary<string, object> data,
            Func<Exception, IDictionary<string, object>> destructureException)
        {
            base.Destructure(exception, data, destructureException);

            var argumentException = (ArgumentException)exception;

            data.Add(nameof(ArgumentException.ParamName), argumentException.ParamName);
        }
    }
}