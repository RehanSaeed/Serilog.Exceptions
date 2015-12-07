namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;

    public interface IExceptionDestructurer
    {
        Type[] TargetTypes { get; }

        void Destructure(
            Exception exception,
            IDictionary<string, object> data,
            Func<Exception, IDictionary<string, object>> destructureException);
    }
}
