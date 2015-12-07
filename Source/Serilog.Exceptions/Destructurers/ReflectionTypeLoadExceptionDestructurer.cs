namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class ReflectionTypeLoadExceptionDestructurer : ExceptionDestructurer
    {
        public override Type[] TargetTypes
        {
            get { return new Type[] { typeof(ReflectionTypeLoadException) }; }
        }

        public override void Destructure(
            Exception exception,
            IDictionary<string, object> data,
            Func<Exception, IDictionary<string, object>> destructureException)
        {
            base.Destructure(exception, data, destructureException);

            var reflectionTypeLoadException = (ReflectionTypeLoadException)exception;

            if (reflectionTypeLoadException.LoaderExceptions != null)
            {
                data.Add(
                    nameof(ReflectionTypeLoadException.LoaderExceptions),
                    reflectionTypeLoadException.LoaderExceptions.Select(destructureException).ToList());
            }
        }
    }
}
