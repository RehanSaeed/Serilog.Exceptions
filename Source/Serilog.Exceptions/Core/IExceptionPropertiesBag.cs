namespace Serilog.Exceptions.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// Container for all properties of single exception instance.
    /// All properties must be added before ResultsDictionary is requested.
    /// </summary>
    public interface IExceptionPropertiesBag
    {
        /// <summary>
        /// Results should be collected only once, after all the properties
        /// were added using <see cref="AddProperty"/> method.
        /// </summary>
        IReadOnlyDictionary<string, object> ResultDictionary { get; }

        void AddProperty(string key, object value);
    }
}
