namespace Serilog.Exceptions.Test
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using Xunit;

    public class DictionaryExtensionsTest
    {
        [Fact]
        public void ToStringObjectDictionary_StringObjectDictionary_ReturnsStringObjectDictionary()
        {
            var dictionary = (IDictionary)new Dictionary<string, object>()
            {
                { "Key", "Value" }
            };

            var actual = dictionary.ToStringObjectDictionary(new List<string>());

            Assert.Single(actual);
            Assert.Equal("Key", actual.First().Key);
            Assert.Equal("Value", actual.First().Value);
        }

        [Fact]
        public void ToStringObjectDictionary_ListDictionary_ReturnsStringObjectDictionary()
        {
            var dictionary = (IDictionary)new ListDictionary()
            {
                { "Key", "Value" }
            };

            var actual = dictionary.ToStringObjectDictionary(new List<string>());

            Assert.Single(actual);
            Assert.Equal("Key", actual.First().Key);
            Assert.Equal("Value", actual.First().Value);
        }

        [Fact]
        public void ToStringObjectDictionary_IgnoredPropertiesAreNotAdded()
        {
            var dictionary = (IDictionary)new Dictionary<string, object>()
            {
                { "IgnoredKey", "Value" },
                { "Key", "Value" }
            };

            var actual = dictionary.ToStringObjectDictionary(new List<string>() { "IgnoredKey" });
            Assert.Single(actual);
            Assert.Equal("Key", actual.First().Key);
            Assert.Equal("Value", actual.First().Value);
        }
    }
}
