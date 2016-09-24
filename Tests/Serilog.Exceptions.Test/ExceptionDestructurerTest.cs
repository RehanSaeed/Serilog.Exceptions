namespace Serilog.Exceptions.Test
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Exceptions.Destructurers;
    using Serilog.Formatting;
    using Serilog.Formatting.Json;
    using Xunit;
    using static Destructurers.LogJsonOutputUtils;

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
                Assert.DoesNotContain(targetTypes, t => t.FullName == "System.Management.Instrumentation.InstanceNotFoundException");
                Assert.DoesNotContain(targetTypes, t => t.FullName == "System.Management.Instrumentation.InstrumentationBaseException");
                Assert.DoesNotContain(targetTypes, t => t.FullName == "System.Management.Instrumentation.InstrumentationException");
            }
        }

        [Fact]
        public void ApplicationException_ContainsMessage()
        {
            var applicationException = new ApplicationException("MSG");
            Test_LoggedExceptionContainsProperty(applicationException, "Message", "MSG");
        }

        [Fact]
        public void ApplicationException_ContainsHelpLink()
        {
            var applicationException = new ApplicationException() { HelpLink = "HELP LINK" };
            Test_LoggedExceptionContainsProperty(applicationException, "HelpLink", "HELP LINK");
        }

        [Fact]
        public void ApplicationException_ContainsSource()
        {
            var applicationException = new ApplicationException() { Source = "SOURCE" };
            Test_LoggedExceptionContainsProperty(applicationException, "Source", "SOURCE");
        }

        [Fact]
        public void ApplicationException_WithoutStackTrace_ContainsNullStackTrace()
        {
            var applicationException = new ApplicationException();
            Test_LoggedExceptionContainsProperty(applicationException, "StackTrace", null);
        }

        [Fact]
        public void ApplicationException_ContainsData()
        {
            var applicationException = new ApplicationException();
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
        public void ApplicationException_WithStackTrace_ContainsStackTrace()
        {
            try
            {
                throw new ApplicationException();
            }
            catch (ApplicationException ex)
            {
                Test_LoggedExceptionContainsProperty(ex, "StackTrace", ex.StackTrace.ToString());
            }
        }

        [Fact]
        public void ApplicationException_ContainsType()
        {
            var applicationException = new ApplicationException();
            Test_LoggedExceptionContainsProperty(applicationException, "Type", "System.ApplicationException");
        }

        [Fact]
        public void AggregateException_WithTwoArgumentExceptions_TheyAreSerializedInInnerExceptionsProperty()
        {
            var argumentException1 = new ArgumentException("MSG1", "testParamName1");
            var argumentException2 = new ArgumentException("MSG1", "testParamName2");
            var aggregateException = new AggregateException(argumentException1, argumentException2);

            JObject rootObject = LogAndDestructureException(aggregateException);
            JArray innerExceptions = ExtractInnerExceptionsProperty(rootObject);

            Assert.Equal(2, innerExceptions.Count);
            Assert_ContainsPropertyWithValue(Assert.IsType<JObject>(innerExceptions[0]), "ParamName", "testParamName1");
            Assert_ContainsPropertyWithValue(Assert.IsType<JObject>(innerExceptions[1]), "ParamName", "testParamName2");
        }
    }
}