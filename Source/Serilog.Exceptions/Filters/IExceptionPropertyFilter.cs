namespace Serilog.Exceptions.Filters
{
    using System;

    public interface IExceptionPropertyFilter
    {
        bool ShouldPropertyBeFiltered(Type exceptionType, string propertyName, object value);
    }
}
