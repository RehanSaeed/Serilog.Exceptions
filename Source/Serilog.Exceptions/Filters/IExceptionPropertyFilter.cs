namespace Serilog.Exceptions.Filters;

/// <summary>
/// Interface used for filtering exception properties. Filtering process is global, each property of every
/// exception will go through a configured exception property filter.
/// </summary>
public interface IExceptionPropertyFilter
{
    /// <summary>
    /// Called after the property was discovered and destructured but just before it is added to results.
    /// </summary>
    /// <param name="exception">Exception for which properties are filtered.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">Destructured value of the property.</param>
    /// <returns>Boolean flag indicating whether property will be rejected.</returns>
    bool ShouldPropertyBeFiltered(Exception exception, string propertyName, object? value);
}
