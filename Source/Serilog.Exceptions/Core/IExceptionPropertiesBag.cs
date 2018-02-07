namespace Serilog.Exceptions.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// Container for all properties of single exception instance.
    /// All properties must be added before result dictionary is requested.
    /// </summary>
    public interface IExceptionPropertiesBag
    {
        /// <summary>
        /// Results should be collected only once, after all the properties
        /// were added using <see cref="AddProperty"/> method.
        /// </summary>
        /// <returns>
        /// Dictionary with all the properties names and values that were added.
        /// </returns>
        IReadOnlyDictionary<string, object> GetResultDictionary();

        /// <summary>
        /// Adds a property to the bag.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        void AddProperty(string key, object value);
    }
}
