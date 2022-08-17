namespace Serilog.Exceptions.Test.Destructurers;

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json.Linq;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Filters;
using Xunit;
using static LogJsonOutputUtils;

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
        var exception = new Exception();
        var filterMock = new Mock<IExceptionPropertyFilter>();

        LogAndDestructureException(exception, new DestructuringOptionsBuilder().WithFilter(filterMock.Object));

        filterMock.Verify(x => x.ShouldPropertyBeFiltered(exception, "StackTrace", null));
    }

    [Fact]
    public void WithoutReflectionBasedDestructurer_CustomExceptionIsNotLogged()
    {
        var exception = new DictNonScalarKeyException();
        var options = new DestructuringOptionsBuilder().WithoutReflectionBasedDestructurer();

        var rootObject = LogAndDestructureException(exception, options);

        Assert.DoesNotContain(rootObject.Properties(), x => x.Name == "Properties");
    }

    [Fact]
    public void WithoutReflectionBasedDestructurerAndCustomRootName_StandardExceptionIsLogged()
    {
        var exception = new ArgumentException("ARG", "arg");
        var options = new DestructuringOptionsBuilder()
            .WithDefaultDestructurers()
            .WithoutReflectionBasedDestructurer()
            .WithRootName("CUSTOM-ROOT");

        var rootObject = LogAndDestructureException(exception, options);

        var exceptionObject = ExtractExceptionDetails(rootObject, "CUSTOM-ROOT");
        var paramObject = Assert.Single(exceptionObject.Properties(), x => x.Name == "ParamName");
        var jsonValue = Assert.IsType<JValue>(paramObject.Value);
        Assert.Equal("arg", jsonValue.Value);
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
        var exception = new DictNonScalarKeyException();
        exception.Reference.Add(new List<int>() { 1, 2, 3 }, "VALUE");

        var result = LogAndDestructureException(exception, new DestructuringOptionsBuilder());

        var exceptionDetails = ExtractExceptionDetails(result);
        var referenceProperties = Assert.IsType<JObject>(exceptionDetails);
        var referenceProperty = Assert.Single(referenceProperties.Properties(), x => x.Name == "Reference");

        var referenceObject = Assert.IsType<JObject>(referenceProperty.Value);
        var property = Assert.Single(referenceObject.Properties());
        Assert.Equal("System.Collections.Generic.List`1[System.Int32]", property.Name);
    }

    [Fact]
    public void WhenExceptionContainsDbContext_ShouldSkipIQueryableProperties()
    {
        using var context = new ExceptionDbContext();
        var exception = new CustomDbContextException("hello world", context);

        var result = LogAndDestructureException(exception, new DestructuringOptionsBuilder());

        var exceptionDetails = ExtractExceptionDetails(result);
        var nameProperty = Assert.Single(exceptionDetails.Properties(), x => x.Name == nameof(CustomDbContextException.Name));

        var jsonValue = Assert.IsType<JValue>(nameProperty.Value);
        Assert.Equal("hello world", jsonValue.Value);

        var contextProperty = Assert.Single(exceptionDetails.Properties(), x => x.Name == nameof(CustomDbContextException.Context));

        var jsonObject = Assert.IsType<JObject>(contextProperty.Value);
        var customerProperty = Assert.Single(jsonObject.Properties(), x => x.Name == nameof(ExceptionDbContext.Customer));

        var jsonValue2 = Assert.IsType<JValue>(customerProperty.Value);
        var value = Assert.IsType<string>(jsonValue2.Value);
        Assert.Equal("IQueryable skipped", value);
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
