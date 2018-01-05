namespace Serilog.Exceptions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public static class DictionaryExtensions
    {
        public static Dictionary<string, object> ToStringObjectDictionary(this IDictionary dictionary, List<string> ignoredProperties)
        {
            var result = new Dictionary<string, object>(dictionary.Count);

            foreach (var key in dictionary.Keys)
            {
                var value = dictionary[key];
                if (value is KeyValuePair<string, string> parsedKeyValuePair)
                {
                    result.AddIfNotIgnored(parsedKeyValuePair.Key, value, ignoredProperties);
                }
                else
                {
                    result.AddIfNotIgnored(key.ToString(), value, ignoredProperties);
                }
            }

            return result;
        }

        public static void AddIfNotIgnored(this IDictionary<string, object> dictionary, string key, object value, List<string> ignoredProperties)
        {
            if (!ignoredProperties.Contains(key))
            {
                dictionary.Add(key, value);
            }
        }
    }
}
