namespace Serilog.Exceptions.Test.Configuration
{
    using System;
    using System.IO;
    using System.Linq;
    using FluentAssertions;
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

        [Fact]
        public void RecursiveException_DestructuringDepthIsLimitedByConfiguredDepth()
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

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("Configuration/limitDestructuringDepthTo1.json")
                .Build();
            var jsonWriter = new StringWriter();
            ILogger logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.Sink(new LogJsonOutputUtils.TestTextWriterSink(jsonWriter, new JsonFormatter()))
                .CreateLogger();

            // Act
            logger.Error(exception, "EXCEPTION MESSAGE");

            // Assert
            // Parent is depth 1
            // First child is depth 2
            var writtenJson = jsonWriter.ToString();
            var jsonObj = JsonConvert.DeserializeObject<object>(writtenJson);
            JObject rootObject = Assert.IsType<JObject>(jsonObj);
            JObject exceptionDetails = LogJsonOutputUtils.ExtractExceptionDetails(rootObject);
            JObject parentNode = exceptionDetails.Properties().Should()
                .ContainSingle(p => p.Name == nameof(RecursiveException.Node))
                .Which.Value.Should().BeOfType<JObject>().Which;
            var parentProperties = parentNode.Properties().ToArray();
            parentProperties.Should().ContainSingle(p => p.Name == "Name").Which.Value.Should().BeOfType<JValue>().Which.Value.Should().Be("PARENT");
            parentProperties.Should().NotContain(p => p.Name == "_typeTag");
            var child = parentProperties.Should()
                .ContainSingle(p => p.Name == "Child")
                .Which.Value.Should().BeOfType<JObject>().Which;
            var childProperties = child.Properties().ToArray();
            childProperties.Should()
                .ContainSingle(p => p.Name == "_typeTag", "typeTag indicates that object was not destructured using Serilog.Exceptions");
        }
#endif
    }
}
