namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Destructurers;
    using Serilog.Exceptions.Filters;
    using Serilog.Formatting;
    using Serilog.Formatting.Json;
    using Xunit;

    public class LogJsonOutputUtils
    {
        public static JObject LogAndDestructureException(
            Exception exception,
            IExceptionPropertyFilter filter = null,
            IDestructuringOptions destructuringOptions = null)
        {
            // Arrange
            var jsonWriter = new StringWriter();

            ILogger logger = new LoggerConfiguration()
                .Enrich.WithExceptionDetails(ExceptionEnricher.DefaultDestructurers, filter, destructuringOptions)
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

        public static void Test_LoggedExceptionContainsProperty(Exception exception, string propertyKey, string propertyValue)
        {
            JObject rootObject = LogAndDestructureException(exception);
            Assert_JObjectContainsPropertiesExceptionDetailsWithProperty(rootObject, propertyKey, propertyValue);
        }

        public static JArray ExtractInnerExceptionsProperty(JObject jObject)
        {
            JObject exceptionDetailValue = ExtractExceptionDetails(jObject);

            JProperty innerExceptionsProperty = Assert.Single(exceptionDetailValue.Properties(), x => x.Name == "InnerExceptions");
            JArray innerExceptionsValue = Assert.IsType<JArray>(innerExceptionsProperty.Value);

            return innerExceptionsValue;
        }

        public static JObject ExtractExceptionDetails(JObject jObject, string rootName = "ExceptionDetail")
        {
            JProperty propertiesProperty = Assert.Single(jObject.Properties(), x => x.Name == "Properties");
            JObject propertiesObject = Assert.IsType<JObject>(propertiesProperty.Value);

            JProperty exceptionDetailProperty = Assert.Single(propertiesObject.Properties(), x => x.Name == rootName);
            JObject exceptionDetailValue = Assert.IsType<JObject>(exceptionDetailProperty.Value);

            return exceptionDetailValue;
        }

        public static void Assert_ContainsPropertyWithValue(
            JObject jObject,
            string propertyKey,
            string propertyValue)
        {
            JProperty paramNameProperty = Assert.Single(jObject.Properties(), x => x.Name == propertyKey);
            JValue paramName = Assert.IsType<JValue>(paramNameProperty.Value);

            Assert.Equal(propertyValue, paramName.Value);
        }

        public static void Assert_JObjectContainsPropertiesExceptionDetailsWithProperty(
            JObject jObject,
            string propertyKey,
            string propertyValue)
        {
            JObject exceptionDetailValue = ExtractExceptionDetails(jObject);
            Assert_ContainsPropertyWithValue(exceptionDetailValue, propertyKey, propertyValue);
        }

        internal class TestTextWriterSink : ILogEventSink
        {
            private readonly TextWriter textWriter;
            private readonly ITextFormatter textFormatter;

            public TestTextWriterSink(TextWriter textWriter, ITextFormatter textFormatter)
            {
                if (textFormatter == null)
                {
                    throw new ArgumentNullException(nameof(textFormatter));
                }

                this.textWriter = textWriter;
                this.textFormatter = textFormatter;
            }

            public void Emit(LogEvent logEvent)
            {
                this.textFormatter.Format(logEvent, this.textWriter);
                this.textWriter.Flush();
            }
        }
    }
}
