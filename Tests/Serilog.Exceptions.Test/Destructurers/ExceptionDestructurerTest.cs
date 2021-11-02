namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json.Linq;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Destructurers;
    using Serilog.Exceptions.Filters;
    using Xunit;
    using static LogJsonOutputUtils;

#pragma warning disable CA2208 // Instantiate argument exceptions correctly
    public class ExceptionDestructurerTest
    {
        [Fact]
        public void TargetTypes()
        {
            var destructurer = new ExceptionDestructurer();

            var targetTypes = destructurer.TargetTypes;

            if (Type.GetType("System.Diagnostics.Eventing.Reader.EventLogInvalidDataException, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") is not null)
            {
                // Full .NET
                targetTypes.Should().Contain(t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogInvalidDataException");
                targetTypes.Should().Contain(t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogNotFoundException");
                targetTypes.Should().Contain(t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogProviderDisabledException");
                targetTypes.Should().Contain(t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogReadingException");
                targetTypes.Should().Contain(t => t.FullName == "System.Diagnostics.Tracing.EventSourceException");
                targetTypes.Should().Contain(t => t.FullName == "System.Management.Instrumentation.InstanceNotFoundException");
                targetTypes.Should().Contain(t => t.FullName == "System.Management.Instrumentation.InstrumentationBaseException");
                targetTypes.Should().Contain(t => t.FullName == "System.Management.Instrumentation.InstrumentationException");
            }
            else
            {
                // Mono
                targetTypes.Should().NotContain(t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogInvalidDataException");
                targetTypes.Should().NotContain(t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogNotFoundException");
                targetTypes.Should().NotContain(t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogProviderDisabledException");
                targetTypes.Should().NotContain(t => t.FullName == "System.Diagnostics.Eventing.Reader.EventLogReadingException");
                targetTypes.Should().NotContain(t => t.FullName == "System.Diagnostics.Tracing.EventSourceException");
                targetTypes.Should().NotContain(t => t.FullName == "System.Management.Instrumentation.InstanceNotFoundException");
                targetTypes.Should().NotContain(t => t.FullName == "System.Management.Instrumentation.InstrumentationBaseException");
                targetTypes.Should().NotContain(t => t.FullName == "System.Management.Instrumentation.InstrumentationException");
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
            Test_LoggedExceptionContainsProperty(applicationException, "StackTrace", null!);
        }

        [Fact]
        public void ArgumentException_ContainsData()
        {
            var applicationException = new ArgumentException();
            applicationException.Data["SOMEKEY"] = "SOMEVALUE";

            var rootObject = LogAndDestructureException(applicationException);
            var exceptionDetail = ExtractExceptionDetails(rootObject);

            var dataProperty = Assert.Single(exceptionDetail.Properties(), x => x.Name == "Data");
            var dataObject = Assert.IsType<JObject>(dataProperty.Value);

            var someKeyProperty = Assert.Single(dataObject.Properties(), x => x.Name == "SOMEKEY");
            var someKeyValue = Assert.IsType<JValue>(someKeyProperty.Value);
            Assert.Equal("SOMEVALUE", someKeyValue.Value);
        }

        [Fact]
        public void ArgumentException_WithCustomRootName_ContainsDataInCustomRootName()
        {
            const string customRootName = "Ex";
            var applicationException = new ArgumentException();
            applicationException.Data["SOMEKEY"] = "SOMEVALUE";

            var rootObject = LogAndDestructureException(
                applicationException,
                destructuringOptions: new DestructuringOptionsBuilder().WithDefaultDestructurers().WithRootName(customRootName));
            var exceptionDetail = ExtractExceptionDetails(rootObject, customRootName);

            Assert.Single(exceptionDetail.Properties(), x => x.Name == "Data");
        }

        [Fact]
        public void PassedFilter_IsCalledWithCorrectArguments()
        {
            // Arrange
            var exception = new Exception();
            var filterMock = new Mock<IExceptionPropertyFilter>();

            // Act
            LogAndDestructureException(exception, new DestructuringOptionsBuilder().WithFilter(filterMock.Object));

            // Assert
            filterMock.Verify(x => x.ShouldPropertyBeFiltered(exception, "StackTrace", null));
        }

        [Fact]
        public void WithoutReflectionBasedDestructurer_CustomExceptionIsNotLogged()
        {
            // Arrange
            var exception = new DictNonScalarKeyException();
            var options = new DestructuringOptionsBuilder().WithoutReflectionBasedDestructurer();

            // Act
            var rootObject = LogAndDestructureException(exception, options);

            // Assert
            rootObject.Properties().Should().NotContain(x => x.Name == "Properties");
        }

        [Fact]
        public void WithoutReflectionBasedDestructurerAndCustomRootName_StandardExceptionIsLogged()
        {
            // Arrange
            var exception = new ArgumentException("ARG", "arg");
            var options = new DestructuringOptionsBuilder()
                .WithDefaultDestructurers()
                .WithoutReflectionBasedDestructurer()
                .WithRootName("CUSTOM-ROOT");

            // Act
            var rootObject = LogAndDestructureException(exception, options);

            // Assert
            var exceptionObject = ExtractExceptionDetails(rootObject, "CUSTOM-ROOT");
            var paramObject = exceptionObject.Properties().Should().ContainSingle(x => x.Name == "ParamName").Which;
            paramObject.Value.Should().BeOfType<JValue>().Which.Value.Should().Be("arg");
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
                Test_LoggedExceptionContainsProperty(ex, "StackTrace", ex.StackTrace?.ToString(CultureInfo.InvariantCulture));
            }
        }

        [Fact]
        public void ArgumentException_ContainsType()
        {
            var applicationException = new ArgumentException();
            Test_LoggedExceptionContainsProperty(applicationException, "Type", "System.ArgumentException");
        }

        [Fact]
        public void WhenExceptionContainsDictionaryWithNonScalarValue_ShouldNotThrow()
        {
            // Arrange
            var exception = new DictNonScalarKeyException();
            exception.Reference.Add(new List<int>() { 1, 2, 3 }, "VALUE");

            // Act
            var result = LogAndDestructureException(exception, new DestructuringOptionsBuilder());

            // Assert
            var exceptionDetails = ExtractExceptionDetails(result);
            var referenceProperty = exceptionDetails.Should().BeOfType<JObject>().Which
                .Properties().Should().ContainSingle(x => x.Name == "Reference").Which;

            var referenceObject = referenceProperty.Value.Should().BeOfType<JObject>().Which;
            var kvp = referenceObject.Properties().Should().ContainSingle()
                .Which.Should().BeOfType<JProperty>()
                .Which.Name.Should().Be("System.Collections.Generic.List`1[System.Int32]");
        }

        private class DefaultExceptionDestructurer<T> : ExceptionDestructurer
        {
            public override Type[] TargetTypes { get; } = { typeof(T) };
        }

        [Fact]
        public void GivenException_ContainingProperty_WithCustomDestructuringPolicy_ShouldApplyThePolicy()
        {
            // Arrange
            var options = new DestructuringOptionsBuilder()
                .WithDefaultDestructurers()
                .WithDestructurers(new[]
                {
                    new DefaultExceptionDestructurer<TokenException>(),
                });
            var token = new Token(1, "Don't show me!");
            var exception = new TokenException();
            exception.Data["@Token"] = token;
            static LoggerConfiguration LoggerConf(LoggerConfiguration x) =>
                x.Destructure.ByTransforming<Token>(x => new { x.Id, Token = new string('*', x.Value.Length) });

            // Act
            var rootObject = LogAndDestructureException(exception, options, LoggerConf);

            // Assert
            var exceptionObject = ExtractExceptionDetails(rootObject);
            var dataObject = exceptionObject.Properties().Should().ContainSingle(x => x.Name == "Data").Which;
            var tokenObject = dataObject
                .Should().BeOfType<JProperty>().Which.Value
                .Should().BeOfType<JObject>()
                .Which.Properties().Should().ContainSingle(x => x.Name == "@Token").Which;
            tokenObject.Value.Should().BeOfType<JObject>().Which
                .Properties().Should().ContainSingle(x => x.Name == "Token").Which
                .Should().BeOfType<JProperty>().Which.Value
                .Should().BeOfType<JValue>().Which.Value.Should().Be("**************");
        }

        private class Token
        {
            public Token(int id, string value)
            {
                this.Id = id;
                this.Value = value;
            }

            public int Id { get; private init; }

            public string Value { get; private init; }
        }

        class TokenException : Exception { }

        public class DictNonScalarKeyException : Exception
        {
            public DictNonScalarKeyException() => this.Reference = new Dictionary<IEnumerable<int>, object>();

            public DictNonScalarKeyException(string message)
                : base(message) =>
                this.Reference = new Dictionary<IEnumerable<int>, object>();

            public DictNonScalarKeyException(string message, Exception innerException)
                : base(message, innerException) =>
                this.Reference = new Dictionary<IEnumerable<int>, object>();

            public Dictionary<IEnumerable<int>, object> Reference { get; }
        }
    }
}
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
