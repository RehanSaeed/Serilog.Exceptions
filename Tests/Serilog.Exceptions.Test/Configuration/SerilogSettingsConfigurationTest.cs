namespace Serilog.Exceptions.Test.Configuration
{
    using System;
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Test.Destructurers;
    using Serilog.Formatting.Json;
    using Xunit;

    public class SerilogSettingsConfigurationTest
    {
#if NETCOREAPP2_0
        [Fact]
        public void ArgumentException_WithCustomRootName_ContainsDataInCustomRootName()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("Configuration/customRootName.json")
                .Build();
            var jsonWriter = new StringWriter();
            ILogger logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.Sink(new LogJsonOutputUtils.TestTextWriterSink(jsonWriter, new JsonFormatter()))
                .CreateLogger();

            var exception = new ArgumentException();
            const string customRootName = "Ex";

            // Act
            logger.Error(exception, "EXCEPTION MESSAGE");

            // Assert
            var writtenJson = jsonWriter.ToString();
            var jsonObj = JsonConvert.DeserializeObject<object>(writtenJson);
            JObject rootObject = Assert.IsType<JObject>(jsonObj);
            LogJsonOutputUtils.ExtractExceptionDetails(rootObject, customRootName);
        }
#endif
    }
}
