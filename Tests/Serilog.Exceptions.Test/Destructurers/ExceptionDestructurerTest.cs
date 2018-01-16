namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Destructurers;
    using Xunit;
    using static LogJsonOutputUtils;

    public class ExceptionDestructurerTest
    {
        [Fact]
        public void TargetTypes()
        {
            var destructurer = new ExceptionDestructurer();

            var targetTypes = destructurer.TargetTypes;

            if (Type.GetType("System.Diagnostics.Eventing.Reader.EventLogInvalidDataException, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") != null)
            {
                // Full .NET
                Assert.Contains(targetTypes, t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogInvalidDataException");
                Assert.Contains(targetTypes, t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogNotFoundException");
                Assert.Contains(targetTypes, t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogProviderDisabledException");
                Assert.Contains(targetTypes, t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogReadingException");
                Assert.Contains(targetTypes, t => t.FullName == "System.Diagnostics.Tracing.EventSourceException");
                Assert.Contains(targetTypes, t => t.FullName == "System.Management.Instrumentation.InstanceNotFoundException");
                Assert.Contains(targetTypes, t => t.FullName == "System.Management.Instrumentation.InstrumentationBaseException");
                Assert.Contains(targetTypes, t => t.FullName == "System.Management.Instrumentation.InstrumentationException");
            }
            else
            {
                // Mono
                Assert.DoesNotContain(targetTypes, t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogInvalidDataException");
                Assert.DoesNotContain(targetTypes, t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogNotFoundException");
                Assert.DoesNotContain(targetTypes, t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogProviderDisabledException");
                Assert.DoesNotContain(targetTypes, t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogReadingException");
                Assert.DoesNotContain(targetTypes, t => t.FullName == "System.Diagnostics.Tracing.EventSourceException");
                Assert.DoesNotContain(targetTypes, t => t.FullName == "System.Management.Instrumentation.InstanceNotFoundException");
                Assert.DoesNotContain(targetTypes, t => t.FullName == "System.Management.Instrumentation.InstrumentationBaseException");
                Assert.DoesNotContain(targetTypes, t => t.FullName == "System.Management.Instrumentation.InstrumentationException");
            }
        }

        [Fact]
        public void ArgumentException_ContainsMessage()
        {
            var applicationException = new ArgumentException("MSG");
            Test_LoggedExceptionContainsProperty(applicationException, "Message", "MSG");
        }

        [Fact]
        public void ArgumentException_ContainsHelpLink()
        {
            var applicationException = new ArgumentException() { HelpLink = "HELP LINK" };
            Test_LoggedExceptionContainsProperty(applicationException, "HelpLink", "HELP LINK");
        }

        [Fact]
        public void ArgumentException_ContainsSource()
        {
            var applicationException = new ArgumentException() { Source = "SOURCE" };
            Test_LoggedExceptionContainsProperty(applicationException, "Source", "SOURCE");
        }

        [Fact]
        public void ArgumentException_WithoutStackTrace_ContainsNullStackTrace()
        {
            var applicationException = new ArgumentException();
            Test_LoggedExceptionContainsProperty(applicationException, "StackTrace", null);
        }

        [Fact]
        public void ArgumentException_ContainsData()
        {
            var applicationException = new ArgumentException();
            applicationException.Data["SOMEKEY"] = "SOMEVALUE";

            JObject rootObject = LogAndDestructureException(applicationException);
            JObject exceptionDetail = ExtractExceptionDetails(rootObject);

            JProperty dataProperty = Assert.Single(exceptionDetail.Properties(), x => x.Name == "Data");
            JObject dataObject = Assert.IsType<JObject>(dataProperty.Value);

            JProperty someKeyProperty = Assert.Single(dataObject.Properties(), x => x.Name == "SOMEKEY");
            JValue someKeyValue = Assert.IsType<JValue>(someKeyProperty.Value);
            Assert.Equal("SOMEVALUE", someKeyValue.Value);
        }

        [Fact]
        public void ArgumentException_WithStackTrace_ContainsStackTrace()
        {
            try
            {
                throw new ArgumentException();
            }
            catch (ArgumentException ex)
            {
                Test_LoggedExceptionContainsProperty(ex, "StackTrace", ex.StackTrace.ToString());
            }
        }

        [Fact]
        public void ArgumentException_ContainsType()
        {
            var applicationException = new ArgumentException();
            Test_LoggedExceptionContainsProperty(applicationException, "Type", "System.ArgumentException");
        }

        [Fact]
        public void ExceptionWithTypeProperty_StillContainsType_JustWithDollarAsPrefixInLabel()
        {
            var exceptionWithTypeProperty = new ExceptionWithTypeProperty() { Type = 13 };
            Test_LoggedExceptionContainsProperty(exceptionWithTypeProperty, "$Type", "Serilog.Exceptions.Test.Destructurers.ExceptionDestructurerTest+ExceptionWithTypeProperty");
        }

        [Fact]
        public void When_object_contains_cyclic_references_then_no_stackoverflow_exception_is_thrown()
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
            var destructurer = new ReflectionBasedDestructurer();
            destructurer.Destructure(exception, result, null);

            // Assert
            var myObject = (Dictionary<string, object>)result.GetResultDictionary()["MyObject"];

            Assert.Equal("bar", myObject["Foo"]);
            Assert.Equal(myObject["$id"], ((Dictionary<string, object>)myObject["Reference"])["$ref"]);
            Assert.Equal(myObject["$id"], ((Dictionary<string, object>)myObject["Reference2"])["$ref"]);
            Assert.Equal("1", myObject["$id"]);
        }

        [Fact]
        public void When_object_contains_cyclic_references_in_list_then_recursive_destructure_is_immediately_stopped()
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
            var destructurer = new ReflectionBasedDestructurer();
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
        public void When_object_contains_cyclic_references_in_dict_then_recursive_destructure_is_immediately_stopped()
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
            var destructurer = new ReflectionBasedDestructurer();
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
    }
}
