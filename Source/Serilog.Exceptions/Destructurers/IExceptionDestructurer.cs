namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;
    using Serilog.Exceptions.Core;

    public interface IExceptionDestructurer
    {
        Type[] TargetTypes { get; }

        void Destructure(
            Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException);
    }
}
