namespace Serilog.Exceptions
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Helper extension methods for specific dictionary operations.
    /// </summary>
    internal static class DictionaryExtensions
    {
        /// <summary>
        /// Converts a dictionary to another one with string-ified keys.
        /// </summary>
        /// <param name="dictionary">The input dictionary.</param>
        /// <returns>A dictionary with string-ified keys.</returns>
        public static Dictionary<string, object> ToStringObjectDictionary(this IDictionary dictionary)
        {
            var result = new Dictionary<string, object>(dictionary.Count);

            foreach (var key in dictionary.Keys)
            {
                var value = dictionary[key];

                result.Add(key.ToString(), value);
            }

            return result;
        }
    }
}
