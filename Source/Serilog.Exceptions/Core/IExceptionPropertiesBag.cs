namespace Serilog.Exceptions.Core
{
    using System.Collections.Generic;

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
