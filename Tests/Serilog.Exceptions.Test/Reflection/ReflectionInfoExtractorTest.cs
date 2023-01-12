namespace Serilog.Exceptions.Test.Reflection;

using System.Linq;
using Serilog.Exceptions.Reflection;
using Xunit;

public class ReflectionInfoExtractorTest
{
    private readonly ReflectionInfoExtractor reflectionInfoExtractor = new();

    [Fact]
    public void GivenObjectWithRedefinedProperty_ShouldDiscardBaseClassProperty()
    {
        var testObject = new TestObjectWithRedefinedProperty() { Name = 123 };

        var reflectionInfo = this.reflectionInfoExtractor.GetOrCreateReflectionInfo(typeof(TestObjectWithRedefinedProperty));

        Assert.Equal(2, reflectionInfo.Properties.Length);

        var namePropertyInfo = Assert.Single(reflectionInfo.Properties, x => x.Name == "Name");
        Assert.Equal(nameof(TestObjectWithRedefinedProperty.Name), namePropertyInfo.Name);
        Assert.Equal(typeof(TestObjectWithRedefinedProperty), namePropertyInfo.DeclaringType);
        var nameGetter = namePropertyInfo.Getter;
        var testObjectName = nameGetter(testObject);
        var integer = Assert.IsType<int>(testObjectName);
        Assert.Equal(123, integer);

        var baseClassPropertyInfo = Assert.Single(reflectionInfo.Properties, x => x.Name == "TestObject.Name");
        var baseClassNameGetter = baseClassPropertyInfo.Getter;
        var baseClassTestObjectName = baseClassNameGetter(testObject);
        Assert.Null(baseClassTestObjectName);
    }

    [Fact]
    public void GivenObjectWithDoubleRedefinedProperty_ShouldMarkBaseClassPropertiesWithFullName()
    {
        var testObject = new TestObjectWithDoubleRedefinedProperty() { Name = 456.789 };

        var reflectionInfo = this.reflectionInfoExtractor.GetOrCreateReflectionInfo(typeof(TestObjectWithDoubleRedefinedProperty));

        var propertyNames = reflectionInfo.Properties
            .Select(x => x.Name)
            .ToList();
        Assert.Equivalent(
            new[]
            {
                "Name",
                "TestObjectWithRedefinedProperty.Name",
                "TestObject.Name",
            },
            propertyNames);

        var namePropertyInfo = Assert.Single(reflectionInfo.Properties, x => x.Name == "Name");
        Assert.Equal(nameof(TestObjectWithDoubleRedefinedProperty.Name), namePropertyInfo.Name);
        Assert.Equal(typeof(TestObjectWithDoubleRedefinedProperty), namePropertyInfo.DeclaringType);
        var nameGetter = namePropertyInfo.Getter;
        var testObjectName = nameGetter(testObject);
        var number = Assert.IsType<double>(testObjectName);
        Assert.Equal(456.789, number);
    }

    public class TestObject
    {
        public string? Name { get; set; }
    }

    public class TestObjectWithRedefinedProperty : TestObject
    {
        public new int Name { get; set; }
    }

    public class TestObjectWithDoubleRedefinedProperty : TestObjectWithRedefinedProperty
    {
        public new double Name { get; set; }
    }
}
