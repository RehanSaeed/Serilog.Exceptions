namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Filters;
    using Xunit;

    public class ExceptionPropertiesBagTest
    {
        [Fact]
        public void Constructor_GivenNullException_Throws()
        {
            // Arrange
            static void Action() => new ExceptionPropertiesBag(null);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(Action);

            // Assert
            Assert.Equal("exception", ex.ParamName);
        }

        [Fact]
        public void AddedProperty_IsAvailableInReturnedDictionary()
        {
            // Arrange
            var properties = new ExceptionPropertiesBag(new Exception(), null);

            // Act
            properties.AddProperty("key", "value");

            // Assert
            var results = properties.GetResultDictionary();
            Assert.Equal(1, results.Count);
            Assert.Contains("key", results.Keys);
            var value = results["key"];
            Assert.Equal("value", value);
        }

        [Fact]
        public void CannotAddProperty_WhenResultWasAlreadyAquired()
        {
            // Arrange
            var properties = new ExceptionPropertiesBag(new Exception(), null);
            properties.AddProperty("key", "value");
            var results = properties.GetResultDictionary();

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => properties.AddProperty("key2", "value2"));

            // Assert
            Assert.Equal("Cannot add exception property 'key2' to bag, after results were already collected", ex.Message);
        }

        [Fact]
        public void CannotAddProperty_WhenKeyIsNull()
        {
            // Arrange
            var properties = new ExceptionPropertiesBag(new Exception(), null);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => properties.AddProperty(null, "value"));

            // Assert
            Assert.StartsWith("Cannot add exception property without a key", ex.Message);
            Assert.Equal("key", ex.ParamName);
        }

        [Fact]
        public void AddedProperty_WhenFilterIsSetToIgnoreIt_IsSkipped()
        {
            // Arrange
            var properties = new ExceptionPropertiesBag(
                new Exception(),
                new IgnorePropertyByNameExceptionFilter(new[] { "key" }));

            // Act
            properties.AddProperty("key", "value");

            // Assert
            var results = properties.GetResultDictionary();
            Assert.Equal(0, results.Count);
        }

        [Fact]
        public void AddedProperty_WhenFilterIsNotSetToIgnoreIt_IsIncluded()
        {
            // Arrange
            var properties = new ExceptionPropertiesBag(
                new Exception(),
                new IgnorePropertyByNameExceptionFilter(new[] { "not key" }));

            // Act
            properties.AddProperty("key", "value");

            // Assert
            var results = properties.GetResultDictionary();
            Assert.Equal(1, results.Count);
            Assert.Contains("key", results.Keys);
            var value = results["key"];
            Assert.Equal("value", value);
        }
    }
}
