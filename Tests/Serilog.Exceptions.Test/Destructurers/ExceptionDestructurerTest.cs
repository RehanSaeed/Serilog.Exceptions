using System.Collections;
using System.Linq;

namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
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
        public void When_object_contains_cyclic_references_then_no_stackoverflow_exception_is_thrown()
        {
            // Arrange
            var exception = new CyclicException();
            exception.MyObject = new MyObject();
            exception.MyObject.Foo = "bar";
            exception.MyObject.Reference = exception.MyObject;

            // Act
            var result = new Dictionary<string, object>();
            var destructurer = new ReflectionBasedDestructurer();
            destructurer.Destructure(exception, result, null);

            // Assert
            var myObject = (Dictionary<string, object>)result["MyObject"];

            Assert.Equal("bar", myObject["Foo"]);
            Assert.Equal(myObject["$id"], ((Dictionary<string, object>)myObject["Reference"])["$ref"]);
        }

        [Fact]
        public void When_object_contains_cyclic_references_in_list_then_recursive_destructure_is_immediately_stopped()
        {
            // Arrange
            var cyclic = new MyObject2();
            cyclic.Foo = "Cyclic";
            cyclic.Reference = cyclic;

            var exception = new CyclicException2();
            exception.MyObject2 = new MyObject2();
            exception.MyObject2.Foo = "bar";
            exception.MyObject2.Reference = cyclic;

            // Act
            var result = new Dictionary<string, object>();
            var destructurer = new ReflectionBasedDestructurer();
            destructurer.Destructure(exception, result, null);

            // Assert
            var myObject = (List<object>)result["MyObject2"];

            // exception.MyObject2[0] is still list
            var firstLevelList = Assert.IsType<List<object>>(myObject[0]);

            // exception.MyObject2[0][0] we notice that we would again destructure "cyclic"
            var secondLevelList = Assert.IsType<Dictionary<string, object>>(firstLevelList[0]);
            Assert.Equal("cyclic ref", secondLevelList["$ref"]);
        }

        public class MyObject
        {
            public string Foo { get; set; }

            public MyObject Reference { get; set; }
        }

        public class CyclicException : Exception
        {
            public MyObject MyObject { get; set; }
        }

        public class MyObject2 : IEnumerable<MyObject2>
        {
            public string Foo { get; set; }

            public MyObject2 Reference { get; set; }

            public IEnumerator<MyObject2> GetEnumerator()
            {
                var myObjects = new List<MyObject2> { this.Reference };
                return myObjects.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        public class CyclicException2 : Exception
        {
            public MyObject2 MyObject2 { get; set; }
        }
    }
}
