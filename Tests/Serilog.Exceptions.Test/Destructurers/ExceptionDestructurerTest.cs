namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Newtonsoft.Json.Linq;
    using NSubstitute;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Destructurers;
    using Serilog.Exceptions.Filters;
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
            Test_LoggedExceptionContainsProperty(applicationException, "StackTrace", null);
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
            var filter = Substitute.For<IExceptionPropertyFilter>();

            // Act
            LogAndDestructureException(exception, new DestructuringOptionsBuilder().WithFilter(filter));

            // Assert
            filter.Received().ShouldPropertyBeFiltered(exception, "StackTrace", null);
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
        public void WhenExceptionContainsDictionaryWithNonScalarValue_ShouldNotThrow()
        {
            // Arrange
            var exception = new ExceptionWithDictNonScalarKey()
            {
                Reference = new Dictionary<IEnumerable<int>, object>()
                {
                    { new List<int>() { 1, 2, 3 }, "VALUE" }
                }
            };

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

        public class ExceptionWithDictNonScalarKey : Exception
        {
            public Dictionary<IEnumerable<int>, object> Reference { get; set; }
        }
    }
}
