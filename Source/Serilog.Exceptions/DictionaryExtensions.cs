namespace Serilog.Exceptions
{
    using System.Collections;
    using System.Collections.Generic;

    internal static class DictionaryExtensions
    {
        public static Dictionary<string, object> ToStringObjectDictionary(this IDictionary dictionary)
        {
            var result = new Dictionary<string, object>();

            foreach (var key in dictionary.Keys)
            {
                var value = dictionary[key];

                result.Add(key.ToString(), value);
            }

            return result;
        }
    }
}
