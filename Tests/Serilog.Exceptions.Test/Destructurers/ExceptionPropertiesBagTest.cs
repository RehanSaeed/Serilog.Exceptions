using System;

namespace Serilog.Exceptions.Test.Destructurers
{
    using Serilog.Exceptions.Destructurers;
    using Xunit;

    public class ExceptionPropertiesBagTest
    {
        [Fact]
        public void AddedProperty_IsAvailableInReturnedDictionary()
        {
            // Arrange
            var properties = new ExceptionPropertiesBag();

            // Act
            properties.AddProperty("key", "value");

            // Assert
            var results = properties.ResultDictionary;
            Assert.Equal(1, results.Count);
            Assert.Contains("key", results.Keys);
            var value = results["key"];
            Assert.Equal("value", value);
        }

        [Fact]
        public void CannotAddProperty_WhenResultWasAlreadyAquired()
        {
            // Arrange
            var properties = new ExceptionPropertiesBag();
            properties.AddProperty("key", "value");
            var results = properties.ResultDictionary;

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => properties.AddProperty("key2", "value2"));

            // Assert
            Assert.Equal("Cannot add exception property 'key2' to bag, after results were already collected", ex.Message);
        }
    }
}
