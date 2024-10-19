namespace Serilog.Exceptions.Test.Destructurers;

using Serilog.Exceptions.Core;
using Serilog.Exceptions.Filters;
using Xunit;

public class ExceptionPropertiesBagTest
{
    [Fact]
    public void Constructor_GivenNullException_Throws()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new ExceptionPropertiesBag(null!));

        Assert.Equal("exception", ex.ParamName);
    }

    [Fact]
    public void AddedProperty_IsAvailableInReturnedDictionary()
    {
        var properties = new ExceptionPropertiesBag(new Exception(), null);

        properties.AddProperty("key", "value");

        var results = properties.GetResultDictionary();
        Assert.Equal(1, results.Count);
        Assert.Contains("key", results.Keys);
        var value = results["key"];
        Assert.Equal("value", value);
    }

    [Fact]
    public void CannotAddProperty_WhenResultWasAlreadyAquired()
    {
        var properties = new ExceptionPropertiesBag(new Exception(), null);
        properties.AddProperty("key", "value");
        var results = properties.GetResultDictionary();

        var ex = Assert.Throws<InvalidOperationException>(() => properties.AddProperty("key2", "value2"));

        Assert.Equal("Cannot add exception property 'key2' to bag, after results were already collected", ex.Message);
    }

    [Fact]
    public void CannotAddProperty_WhenKeyIsNull()
    {
        var properties = new ExceptionPropertiesBag(new Exception(), null);

        var ex = Assert.Throws<ArgumentNullException>(() => properties.AddProperty(null!, "value"));

        Assert.Equal("key", ex.ParamName);
    }

    [Fact]
    public void AddedProperty_WhenFilterIsSetToIgnoreIt_IsSkipped()
    {
        var properties = new ExceptionPropertiesBag(
            new Exception(),
            new IgnorePropertyByNameExceptionFilter(["key"]));

        properties.AddProperty("key", "value");

        var results = properties.GetResultDictionary();
        Assert.Equal(0, results.Count);
    }

    [Fact]
    public void AddedProperty_WhenFilterIsNotSetToIgnoreIt_IsIncluded()
    {
        var properties = new ExceptionPropertiesBag(
            new Exception(),
            new IgnorePropertyByNameExceptionFilter(["not key"]));

        properties.AddProperty("key", "value");

        var results = properties.GetResultDictionary();
        Assert.Equal(1, results.Count);
        Assert.Contains("key", results.Keys);
        var value = results["key"];
        Assert.Equal("value", value);
    }
}
