namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Destructurers;
    using Xunit;

    public class ReflectionBasedDestructurerTest
    {
        private ReflectionBasedDestructurer destructurer;

        public ReflectionBasedDestructurerTest()
        {
            this.destructurer = new ReflectionBasedDestructurer(10);
        }

        [Fact]
        public void Destructure_()
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

            var propertiesBag = new ExceptionPropertiesBag(new Exception());

            this.destructurer.Destructure(exception, propertiesBag, null);

            var properties = propertiesBag.GetResultDictionary();
            Assert.Equal("PublicValue", properties[nameof(TestException.PublicProperty)]);
            Assert.Equal("threw System.Exception: Exception of type 'System.Exception' was thrown.", properties[nameof(TestException.ExceptionProperty)]);
            Assert.DoesNotContain(properties, x => string.Equals(x.Key, "InternalProperty"));
            Assert.DoesNotContain(properties, x => string.Equals(x.Key, "ProtectedProperty"));
            Assert.DoesNotContain(properties, x => string.Equals(x.Key, "PrivateProperty"));
            Assert.Equal("MessageValue", properties[nameof(TestException.Message)]);
            var data = Assert.IsType<Dictionary<string, object>>(properties[nameof(TestException.Data)]);
            Assert.Empty(data);
            Assert.Null(properties[nameof(TestException.InnerException)]);
#if NET461
            Assert.StartsWith("Void Destructure_(", properties[nameof(TestException.TargetSite)].ToString());
#endif
            Assert.NotEmpty(properties[nameof(TestException.StackTrace)].ToString());
            Assert.Null(properties[nameof(TestException.HelpLink)]);
            Assert.Equal("Serilog.Exceptions.Test", properties[nameof(TestException.Source)]);
            Assert.Equal(-2146233088, properties[nameof(TestException.HResult)]);
            Assert.Contains(typeof(TestException).FullName, properties["Type"].ToString());
        }

        [Fact]
        public void CanDestructureUriProperty()
        {
            const string uriValue = "http://localhost/property";
            var exception = new UriException("test", new Uri(uriValue));

            var propertiesBag = new ExceptionPropertiesBag(exception);
            this.destructurer.Destructure(exception, propertiesBag, null);

            var properties = propertiesBag.GetResultDictionary();
            var uriPropertyValue = properties[nameof(UriException.Uri)];
            Assert.IsType<string>(uriPropertyValue);
            Assert.Equal(uriValue, uriPropertyValue);
        }

        [Fact]
        public void CanDestructureUriDataItem()
        {
            const string uriValue = "http://localhost/data-item";
            var exception = new Exception("test")
            {
                Data =
                {
                    { "UriDataItem", new Uri(uriValue) }
                }
            };

            var propertiesBag = new ExceptionPropertiesBag(exception);
            this.destructurer.Destructure(exception, propertiesBag, null);

            var properties = propertiesBag.GetResultDictionary();
            var data = (IDictionary)properties[nameof(Exception.Data)];
            var uriDataValue = data["UriDataItem"];
            Assert.IsType<string>(uriDataValue);
            Assert.Equal(uriValue, uriDataValue);
        }

        [Fact]
        public void DestructuringDepthIsLimitedByConfiguredDepth()
        {
            // Arrange
            var exception = new RecursiveException()
            {
                Node = new RecursiveNode()
                {
                    Name = "PARENT",
                    Child = new RecursiveNode()
                    {
                        Name = "CHILD 1",
                        Child = new RecursiveNode()
                        {
                            Name = "CHILD 2"
                        }
                    }
                }
            };
            this.destructurer = new ReflectionBasedDestructurer(1);

            // Act
            var propertiesBag = new ExceptionPropertiesBag(exception);
            this.destructurer.Destructure(exception, propertiesBag, null);

            // Assert
            // Parent is depth 1
            // First child is depth 2
            var properties = propertiesBag.GetResultDictionary();
            var parent = (IDictionary<string, object>)properties[nameof(RecursiveException.Node)];
            Assert.Equal("PARENT", parent[nameof(RecursiveNode.Name)]);
            Assert.IsType<RecursiveNode>(parent[nameof(RecursiveNode.Child)]);
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
            }

            public static string StaticProperty { get; set; }

            public string PublicProperty { get; set; }

            public string ExceptionProperty
            {
                get { throw new Exception(); }
            }

            internal string InternalProperty { get; set; }

            protected string ProtectedProperty { get; set; }

            private string PrivateProperty { get; set; }

            public string this[int i]
            {
                get { return "IndexerValue"; }
            }
        }

        public class UriException : Exception
        {
            public UriException(string message, Uri uri)
                : base(message)
            {
                this.Uri = uri;
            }

            public Uri Uri { get; }
        }

        public class RecursiveNode
        {
            public string Name { get; set; }
            public RecursiveNode Child { get; set; }
        }

        public class RecursiveException : Exception
        {
            public RecursiveNode Node { get; set; }
        }
    }
}
