namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;

    public interface IExceptionDestructurer
    {
        Type[] TargetTypes { get; }

        void Destructure(
            Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException);
    }
}
