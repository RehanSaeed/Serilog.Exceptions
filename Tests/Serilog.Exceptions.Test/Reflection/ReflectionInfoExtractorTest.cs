namespace Serilog.Exceptions.Test.Reflection
{
    using System.Linq;
    using Exceptions.Reflection;
    using FluentAssertions;
    using Xunit;

    public class ReflectionInfoExtractorTest
    {
        private readonly ReflectionInfoExtractor reflectionInfoExtractor = new();

        [Fact]
        public void GivenObjectWithRedefinedProperty_ShouldDiscardBaseClassProperty()
        {
            var testObject = new TestObjectWithRedefinedProperty() { Name = 123 };

            var reflectionInfo = this.reflectionInfoExtractor.GetOrCreateReflectionInfo(typeof(TestObjectWithRedefinedProperty));

            reflectionInfo.Properties.Should().HaveCount(2);

            var namePropertyInfo = reflectionInfo.Properties.Should().ContainSingle(x => x.Name == "Name").Which;
            namePropertyInfo.Name.Should().Be(nameof(TestObjectWithRedefinedProperty.Name));
            namePropertyInfo.DeclaringType.Should().Be(typeof(TestObjectWithRedefinedProperty));
            var nameGetter = namePropertyInfo.Getter;
            var testObjectName = nameGetter(testObject);
            testObjectName.Should().BeOfType<int>().Which.Should().Be(123);

            var baseClassPropertyInfo = reflectionInfo
                .Properties.Should().ContainSingle(x => x.Name == "Serilog.Exceptions.Test.Reflection.ReflectionInfoExtractorTest+TestObject.Name").Which;
            var baseClassNameGetter = baseClassPropertyInfo.Getter;
            var baseClassTestObjectName = baseClassNameGetter(testObject);
            baseClassTestObjectName.Should().BeNull();
        }

        [Fact]
        public void GivenObjectWithDoubleRedefinedProperty_ShouldMarkBaseClassPropertiesWithFullName()
        {
            var testObject = new TestObjectWithDoubleRedefinedProperty() { Name = 456.789 };

            var reflectionInfo = this.reflectionInfoExtractor.GetOrCreateReflectionInfo(typeof(TestObjectWithDoubleRedefinedProperty));

            var propertyNames = reflectionInfo.Properties
                .Select(x => x.Name)
                .ToList();
            propertyNames.Should().BeEquivalentTo(
                new[]
                {
                    "Name",
                    "Serilog.Exceptions.Test.Reflection.ReflectionInfoExtractorTest+TestObjectWithRedefinedProperty.Name",
                    "Serilog.Exceptions.Test.Reflection.ReflectionInfoExtractorTest+TestObject.Name",
                },
                x => x.WithoutStrictOrdering());
            var namePropertyInfo = reflectionInfo.Properties.Should().ContainSingle(x => x.Name == "Name").Which;
            namePropertyInfo.Name.Should().Be(nameof(TestObjectWithDoubleRedefinedProperty.Name));
            namePropertyInfo.DeclaringType.Should().Be(typeof(TestObjectWithDoubleRedefinedProperty));
            var nameGetter = namePropertyInfo.Getter;
            var testObjectName = nameGetter(testObject);
            testObjectName.Should().BeOfType<double>().Which.Should().Be(456.789);
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
}
