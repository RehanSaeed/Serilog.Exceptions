namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Destructurers;
    using Xunit;
    using static LogJsonOutputUtils;

    public class ReflectionBasedDestructurerTest
    {
        private ReflectionBasedDestructurer destructurer;

        public ReflectionBasedDestructurerTest()
        {
            this.destructurer = new ReflectionBasedDestructurer(10);
        }

        [Fact]
        public void DestructureComplexException_EachTypeOfPropertyIsDestructuredAsExpected()
        {
            // Arrange
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

            // Act
            this.destructurer.Destructure(exception, propertiesBag, null);

            // Assert
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
            Assert.StartsWith("Void DestructureComplexException_EachTypeOfPropertyIsDestructuredAsExpected(", properties[nameof(TestException.TargetSite)].ToString());
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

        [Fact]
        public void ExceptionWithTypeProperty_StillContainsType_JustWithDollarAsPrefixInLabel()
        {
            var exceptionWithTypeProperty = new ExceptionWithTypeProperty() { Type = 13 };
            Test_LoggedExceptionContainsProperty(exceptionWithTypeProperty, "$Type", $"Serilog.Exceptions.Test.Destructurers.{nameof(ReflectionBasedDestructurerTest)}+ExceptionWithTypeProperty");
        }

        [Fact]
        public void WhenObjectContainsCyclicReferences_ThenNoStackoverflowExceptionIsThrown()
        {
            // Arrange
            var exception = new CyclicException
            {
                MyObject = new MyObject()
            };
            exception.MyObject.Foo = "bar";
            exception.MyObject.Reference = exception.MyObject;
            exception.MyObject.Reference2 = exception.MyObject;

            // Act
            var result = new ExceptionPropertiesBag(new Exception());
            var destructurer = new ReflectionBasedDestructurer(10);
            destructurer.Destructure(exception, result, null);

            // Assert
            var myObject = (Dictionary<string, object>)result.GetResultDictionary()["MyObject"];

            Assert.Equal("bar", myObject["Foo"]);
            Assert.Equal(myObject["$id"], ((Dictionary<string, object>)myObject["Reference"])["$ref"]);
            Assert.Equal(myObject["$id"], ((Dictionary<string, object>)myObject["Reference2"])["$ref"]);
            Assert.Equal("1", myObject["$id"]);
        }

        [Fact]
        public void WhenObjectContainsCyclicReferencesInList_ThenRecursiveDestructureIsImmediatelyStopped()
        {
            // Arrange
            var cyclic = new MyObjectEnumerable
            {
                Foo = "Cyclic"
            };
            cyclic.Reference = cyclic;
            var exception = new CyclicException2
            {
                MyObjectEnumerable = new MyObjectEnumerable()
            };
            exception.MyObjectEnumerable.Foo = "bar";
            exception.MyObjectEnumerable.Reference = cyclic;

            // Act
            var result = new ExceptionPropertiesBag(new Exception());
            var destructurer = new ReflectionBasedDestructurer(10);
            destructurer.Destructure(exception, result, null);

            // Assert
            var myObject = (List<object>)result.GetResultDictionary()["MyObjectEnumerable"];

            // exception.MyObjectEnumerable[0] is still list
            var firstLevelList = Assert.IsType<List<object>>(myObject[0]);

            // exception.MyObjectEnumerable[0][0] we notice that we would again destructure "cyclic"
            var secondLevelList = Assert.IsType<Dictionary<string, object>>(firstLevelList[0]);
            Assert.Equal("Cyclic reference", secondLevelList["$ref"]);
        }

        [Fact]
        public void WhenObjectContainsCyclicReferencesInDict_ThenRecursiveDestructureIsImmediatelyStopped()
        {
            // Arrange
            var cyclic = new MyObjectDict
            {
                Foo = "Cyclic",
                Reference = new Dictionary<string, object>()
            };
            cyclic.Reference["x"] = cyclic.Reference;
            var exception = new CyclicExceptionDict
            {
                MyObjectDict = cyclic
            };

            // Act
            var result = new ExceptionPropertiesBag(new Exception());
            var destructurer = new ReflectionBasedDestructurer(10);
            destructurer.Destructure(exception, result, null);

            // Assert
            var myObject = (Dictionary<string, object>)result.GetResultDictionary()["MyObjectDict"];

            // exception.MyObjectDict["Reference"] is still regular dictionary
            var firstLevelDict = Assert.IsType<Dictionary<string, object>>(myObject["Reference"]);
            var id = firstLevelDict["$id"];
            Assert.Equal("1", id);

            // exception.MyObjectDict["Reference"]["x"] we notice that we are destructuring same dictionary
            var secondLevelDict = Assert.IsType<Dictionary<string, object>>(firstLevelDict["x"]);
            var refId = Assert.IsType<string>(secondLevelDict["$ref"]);
            Assert.Equal(id, refId);
        }

        public class MyObject
        {
            public string Foo { get; set; }

            public MyObject Reference { get; set; }

            public MyObject Reference2 { get; set; }
        }

        public class CyclicException : Exception
        {
            public MyObject MyObject { get; set; }
        }

        public class MyObjectEnumerable : IEnumerable<MyObjectEnumerable>
        {
            public string Foo { get; set; }

            public MyObjectEnumerable Reference { get; set; }

            public IEnumerator<MyObjectEnumerable> GetEnumerator()
            {
                var myObjects = new List<MyObjectEnumerable> { this.Reference };
                return myObjects.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        public class CyclicException2 : Exception
        {
            public MyObjectEnumerable MyObjectEnumerable { get; set; }
        }

        public class CyclicExceptionDict : Exception
        {
            public MyObjectDict MyObjectDict { get; set; }
        }

        public class MyObjectDict
        {
            public string Foo { get; set; }

            public Dictionary<string, object> Reference { get; set; }
        }

        public class ExceptionWithTypeProperty : Exception
        {
            public int Type { get; set; }
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
