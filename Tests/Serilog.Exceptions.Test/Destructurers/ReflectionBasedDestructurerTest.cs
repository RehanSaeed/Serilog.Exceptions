namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Exceptions.Destructurers;
    using Xunit;

    public class ReflectionBasedDestructurerTest
    {
        private readonly ReflectionBasedDestructurer destructurer;

        public ReflectionBasedDestructurerTest()
        {
            this.destructurer = new ReflectionBasedDestructurer(new List<string>());
        }

        [Fact]
        public void ReflectionBasedDestructurer_Destructure()
        {
            var exception = this.GetException();

            var properties = new Dictionary<string, object>();

            this.destructurer.Destructure(exception, properties, null);

            Assert.Equal("PublicValue", properties[nameof(TestException.PublicProperty)]);
            Assert.Equal("threw System.Exception: Exception of type 'System.Exception' was thrown.", properties[nameof(TestException.ExceptionProperty)]);
            Assert.DoesNotContain(properties, x => string.Equals(x.Key, "InternalProperty"));
            Assert.DoesNotContain(properties, x => string.Equals(x.Key, "ProtectedProperty"));
            Assert.DoesNotContain(properties, x => string.Equals(x.Key, "PrivateProperty"));
            Assert.Equal("MessageValue", properties[nameof(TestException.Message)]);

            var nestedProperty = properties[nameof(TestException.NestedProperty)];
            var nestedPair = (Dictionary<string, object>)nestedProperty;
            var nestedKvp = (KeyValuePair<string, string>)nestedPair.First().Value;
            Assert.Equal("NestedPropertyValue", nestedKvp.Value);
            Assert.Equal("NestedPropertyKey", nestedKvp.Key);

            var data = Assert.IsType<Dictionary<string, object>>(properties[nameof(TestException.Data)]);
            Assert.Empty(data);
            Assert.Null(properties[nameof(TestException.InnerException)]);
#if NET461
            Assert.StartsWith("Void ReflectionBasedDestructurer_Destructure(", properties[nameof(TestException.TargetSite)].ToString());
#endif
            Assert.NotEmpty(properties[nameof(TestException.StackTrace)].ToString());
            Assert.Null(properties[nameof(TestException.HelpLink)]);
            Assert.Equal("Serilog.Exceptions.Test", properties[nameof(TestException.Source)]);
            Assert.Equal(-2146233088, properties[nameof(TestException.HResult)]);
            Assert.Contains(typeof(TestException).FullName, properties["Type"].ToString());
        }

        [Fact]
        public void ReflectionBasedDestructurer_PropertiesCanBeIgnored()
        {
            // Arrange
            var exception = this.GetException();
            var properties = new Dictionary<string, object>();
            var localDestructurer = new ReflectionBasedDestructurer(new List<string> { nameof(TestException.Source) });

            // Act
            localDestructurer.Destructure(exception, properties, null);

            // Assert
            Assert.DoesNotContain(properties, x => string.Equals(x.Key, nameof(TestException.Source)));
        }

        [Fact]
        public void ReflectionBasedDestructurer_NestedPropertiesCanBeIgnored()
        {
            // Arrange
            var exception = this.GetException();
            var properties = new Dictionary<string, object>();
            var localDestructurer = new ReflectionBasedDestructurer(new List<string> { "NestedPropertyKey" });

            // Act
            localDestructurer.Destructure(exception, properties, null);
            var nestedProperty = properties[nameof(TestException.NestedProperty)];
            var nestedPair = (Dictionary<string, object>)nestedProperty;

            // Assert
            Assert.NotNull(nestedPair);
            Assert.Empty(nestedPair);
        }

        [Fact]
        public void ReflectionBasedDestructurer_MultiplePropertiesCanBeIgnored()
        {
            var exception = this.GetException();
            var properties = new Dictionary<string, object>();
            var localDestructurer = new ReflectionBasedDestructurer(new List<string> { nameof(TestException.Message), nameof(TestException.Source) });
            localDestructurer.Destructure(exception, properties, null);

            Assert.DoesNotContain(properties.Keys, p => p == nameof(TestException.Message));
            Assert.DoesNotContain(properties.Keys, p => p == nameof(TestException.Source));
        }

        [Fact]
        public void ReflectionBasedDestructurer_MultipleNestedPropertiesCanBeIgnored()
        {
            var exception = this.GetException();
            var properties = new Dictionary<string, object>();
            var localDestructurer = new ReflectionBasedDestructurer(new List<string> { nameof(TestException.Source), "NestedPropertyKey" });
            localDestructurer.Destructure(exception, properties, null);

            var nestedProperty = properties[nameof(TestException.NestedProperty)];
            var nestedPair = (Dictionary<string, object>)nestedProperty;

            // Assert
            Assert.NotNull(nestedPair);
            Assert.Empty(nestedPair);
            Assert.DoesNotContain(properties.Keys, p => p == nameof(TestException.Source));
        }

        private Exception GetException()
        {
            Exception exception;
            try
            {
                throw new TestException();
            }
            catch (Exception e)
            {
                exception = e;
            }

            return exception;
        }

        public class TestException : Exception
        {
            public TestException()
                : base("MessageValue")
            {
                StaticProperty = "StaticValue";
                this.PublicProperty = "PublicValue";
                this.InternalProperty = "InternalValue";
                this.ProtectedProperty = "ProtectedValue";
                this.PrivateProperty = "PrivateValue";

                this.NestedProperty = new Dictionary<string, object>();
                var nestedPropertyKey = "NestedPropertyKey";
                var nestedPropertyValue = "NestedPropertyValue";
                this.NestedProperty.Add("NestedPair", new KeyValuePair<string, string>(nestedPropertyKey, nestedPropertyValue));
            }

            public static string StaticProperty { get; set; }

            public string PublicProperty { get; set; }

            public string ExceptionProperty => throw new Exception();

            public Dictionary<string, object> NestedProperty { get; set; }

            internal string InternalProperty { get; set; }

            protected string ProtectedProperty { get; set; }

            private string PrivateProperty { get; set; }

            public string this[int i] => "IndexerValue";
        }
    }
}
