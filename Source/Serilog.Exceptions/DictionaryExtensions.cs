namespace Serilog.Exceptions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal static class DictionaryExtensions
    {
        public static Dictionary<string, object> ToStringObjectDictionary(this IDictionary dictionary, List<string> ignoredProperties)
        {
            var result = new Dictionary<string, object>();
            foreach (var key in dictionary.Keys)
            {
                var value = dictionary[key];
                if (ignoredProperties.Contains(key.ToString()))
                {
                    continue;
                }

                if (value is KeyValuePair<string, string> parsedKeyValuePair)
                {
                    if (ignoredProperties.Contains(parsedKeyValuePair.Key))
                    {
                        continue;
                    }

                    result.AddIfNotIgnored(key.ToString(), value, ignoredProperties);
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
