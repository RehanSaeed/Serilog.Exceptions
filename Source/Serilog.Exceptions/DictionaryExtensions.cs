namespace Serilog.Exceptions;

using System.Collections;

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
    public static Dictionary<string, object?> ToStringObjectDictionary(this IDictionary dictionary)
    {
        var result = new Dictionary<string, object?>(dictionary.Count);

        foreach (var key in dictionary.Keys)
        {
            if (key is not null)
            {
                var keyString = key.ToString();
                var value = dictionary[key];

                if (keyString is not null)
                {
                    result.Add(keyString, value);
                }
            }
        }

        return result;
    }
}
