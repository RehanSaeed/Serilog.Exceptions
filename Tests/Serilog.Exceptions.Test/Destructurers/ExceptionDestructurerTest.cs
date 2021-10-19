namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Newtonsoft.Json.Linq;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Filters;
    using Xunit;
    using static LogJsonOutputUtils;

#pragma warning disable CA2208 // Instantiate argument exceptions correctly
    public class ExceptionDestructurerTest
    {
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

        [Fact]
        public void WhenExceptionContainsDbContext_ShouldSkipIQueryableProperties()
        {
            // Arrange
            using var context = new ExceptionDbContext();
            var exception = new CustomDbContextException("hello world", context);

            // Act
            var result = LogAndDestructureException(exception, new DestructuringOptionsBuilder());

            // Assert
            var exceptionDetails = ExtractExceptionDetails(result).Should().BeOfType<JObject>().Which;
            var nameProperty = exceptionDetails
                .Properties().Should().ContainSingle(x => x.Name == nameof(CustomDbContextException.Name)).Which
                .Should().BeOfType<JProperty>().Which;

            nameProperty.Value.Should().BeOfType<JValue>().Which.Value.Should().Be("hello world");

            var contextProperty = exceptionDetails
                .Properties().Should().ContainSingle(x => x.Name == nameof(CustomDbContextException.Context)).Which;

            var customerProperty = contextProperty.Value.Should().BeOfType<JObject>().Which
                .Properties().Should().ContainSingle(x => x.Name == nameof(ExceptionDbContext.Customer)).Which;

            customerProperty.Value.Should().BeOfType<JValue>().Which.Value.Should().BeOfType<string>().Which
                .Should().Be("IQueryable skipped");
        }

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

#pragma warning disable CS3001 // Argument type is not CLS-compliant
#pragma warning disable CS3003 // Type is not CLS-compliant
        public class CustomDbContextException : Exception
        {
            public CustomDbContextException(string name, DbContext context)
            {
                this.Name = name;
                this.Context = context;
            }

            public string Name { get; set; }

            public DbContext Context { get; }
        }

        private class ExceptionDbContext : DbContext
        {
            public DbSet<CustomerEntity> Customer => this.Set<CustomerEntity>();

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseInMemoryDatabase(databaseName: "TestDebUpdateException");

            public class CustomerEntity
            {
                public string? Name { get; set; }

                public int Id { get; set; }
            }
        }
    }
}
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
