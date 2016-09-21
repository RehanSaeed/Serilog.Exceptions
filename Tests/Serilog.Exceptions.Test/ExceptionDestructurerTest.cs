using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;

namespace Serilog.Exceptions.Test
{
    using System;
    using Serilog.Exceptions.Destructurers;
    using Xunit;

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
            var applicationException = new ApplicationException() {HelpLink = "HELP LINK"};
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
        public void ApplicationException_WithtStackTrace_ContainsStackTrace()
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
        public void ArgumentException_ParamNameIsAttachedAsProperty()
        {
            var argumentException = new ArgumentException("MSG", "testParamName");
            Test_LoggedExceptionContainsProperty(argumentException, "ParamName", "testParamName");
        }

        [Fact]
        public void ArgumentNullException_ParamNameIsAttachedAsProperty()
        {
            var argumentException = new ArgumentNullException("testParamName", "MSG");
            Test_LoggedExceptionContainsProperty(argumentException, "ParamName", "testParamName");
        }

        [Fact]
        public void ArgumentOfOutRangeException_ParamNameIsAttachedAsProperty()
        {
            var argumentException = new ArgumentOutOfRangeException("testParamName");
            Test_LoggedExceptionContainsProperty(argumentException, "ParamName", "testParamName");
        }

        [Fact]
        public void ArgumentOfOutRangeException_ActualValueIsAttachedAsProperty()
        {
            var argumentException = new ArgumentOutOfRangeException("testParamName", "ACTUAL_VALUE", "MSG");
            Test_LoggedExceptionContainsProperty(argumentException, "ActualValue", "ACTUAL_VALUE");
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

        private JObject LogAndDestructureException(Exception exception)
        {
            // Arrange
            var jsonWriter = new StringWriter();

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithExceptionDetails()
                .WriteTo.Sink(new TestTextWriterSink(jsonWriter, new JsonFormatter()))
                .CreateLogger();

            // Act
            logger.Error(exception, "EXCEPTION MESSAGE");

            // Assert
            var writtenJson = jsonWriter.ToString();
            var jsonObj = JsonConvert.DeserializeObject<object>(writtenJson);
            JObject rootObject = Assert.IsType<JObject>(jsonObj);
            return rootObject;
        }

        private void Test_LoggedExceptionContainsProperty(Exception exception, string propertyKey, string propertyValue)
        {
            JObject rootObject = LogAndDestructureException(exception);
            Assert_JObjectContainsPropertiesExceptionDetailsWithProperty(rootObject, propertyKey, propertyValue);
        }

        private JArray ExtractInnerExceptionsProperty(JObject jObject)
        {
            JObject exceptionDetailValue = ExtractExceptionDetails(jObject);

            JProperty innerExceptionsProperty = Assert.Single(exceptionDetailValue.Properties(), x => x.Name == "InnerExceptions");
            JArray innerExceptionsValue = Assert.IsType<JArray>(innerExceptionsProperty.Value);

            return innerExceptionsValue;
        }

        private JObject ExtractExceptionDetails(JObject jObject)
        {
            JProperty propertiesProperty = Assert.Single(jObject.Properties(), x => x.Name == "Properties");
            JObject propertiesObject = Assert.IsType<JObject>(propertiesProperty.Value);

            JProperty exceptionDetailProperty = Assert.Single(propertiesObject.Properties(), x => x.Name == "ExceptionDetail");
            JObject exceptionDetailValue = Assert.IsType<JObject>(exceptionDetailProperty.Value);

            return exceptionDetailValue;
        }

        private void Assert_ContainsPropertyWithValue(JObject jObject, string propertyKey,
            string propertyValue)
        {
            JProperty paramNameProperty = Assert.Single(jObject.Properties(), x => x.Name == propertyKey);
            JValue paramName = Assert.IsType<JValue>(paramNameProperty.Value);

            Assert.Equal(propertyValue, paramName.Value);
        }

        private void Assert_JObjectContainsPropertiesExceptionDetailsWithProperty(JObject jObject, string propertyKey,
            string propertyValue)
        {
            JObject exceptionDetailValue = ExtractExceptionDetails(jObject);
            Assert_ContainsPropertyWithValue(exceptionDetailValue, propertyKey, propertyValue);
        }

        class TestTextWriterSink : ILogEventSink
        {
            readonly TextWriter _textWriter;
            readonly ITextFormatter _textFormatter;

            public TestTextWriterSink(TextWriter textWriter, ITextFormatter textFormatter)
            {
                if (textFormatter == null) throw new ArgumentNullException(nameof(textFormatter));
                _textWriter = textWriter;
                _textFormatter = textFormatter;
            }

            public void Emit(LogEvent logEvent)
            {
                _textFormatter.Format(logEvent, _textWriter);
                _textWriter.Flush();
            }
        }
    }
}
