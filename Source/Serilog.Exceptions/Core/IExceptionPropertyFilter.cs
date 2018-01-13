namespace Serilog.Exceptions.Core
{
    using System;

    public interface IExceptionPropertyFilter
    {
        bool ShouldPropertyBeFiltered(Type exceptionType, string propertyName, object value);
    }
}
