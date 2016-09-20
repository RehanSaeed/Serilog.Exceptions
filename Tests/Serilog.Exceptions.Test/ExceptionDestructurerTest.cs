using System.Diagnostics;
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
        public void ArgumentExceptions_NameIsAttachedAsProperty()
        {
            // Arrange
            var jsonWriter = new StringWriter();

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithExceptionDetails()
                .WriteTo.Sink(new TestTextWriterSink(jsonWriter, new JsonFormatter()))
                .CreateLogger();

            // Act
            var argumentException = new ArgumentException("abc");
            logger.Error(argumentException, "EXCEPTION MESSAGE");

            // Assert
            var writtenJson = jsonWriter.ToString();
            var jsonObj = JsonConvert.DeserializeObject<object>(writtenJson);

            JObject rootObject = Assert.IsType<JObject>(jsonObj);
            JProperty propertiesProperty = Assert.Single(rootObject.Properties(), x => x.Name == "Properties");
            JObject propertiesObject = Assert.IsType<JObject>(propertiesProperty.Value);
            JProperty exceptionDetailProperty = Assert.Single(propertiesObject.Properties(), x => x.Name == "ExceptionDetail");
            JObject exceptionDetailValue = Assert.IsType<JObject>(exceptionDetailProperty.Value);
            JProperty exceptionTypeProperty = Assert.Single(exceptionDetailValue.Properties(), x => x.Name == "Type");
            JValue exceptionTypeValue = Assert.IsType<JValue>(exceptionTypeProperty.Value);
            Assert.Equal("System.ArgumentException", exceptionTypeValue.Value);
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