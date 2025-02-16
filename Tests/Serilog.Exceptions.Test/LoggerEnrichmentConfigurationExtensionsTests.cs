namespace Serilog.Exceptions.Test;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Castle.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Options;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Destructurers;
using Serilog.Exceptions.Filters;
using Serilog.Exceptions.Grpc.Destructurers;
using Xunit;

public class LoggerEnrichmentConfigurationExtensionsTests
{
    [Fact]
    public void WithExceptionDetails_DefaultOptions()
    {
        // language=json
        const string json = "\"WithExceptionDetails\"";
        var options = ConfigureJson(json);

        Assert.Equal("ExceptionDetail", options.RootName);
        Assert.Equal(10, options.DestructuringDepth);
        Assert.False(options.DisableReflectionBasedDestructurer);
        Assert.Equal(DestructuringOptionsBuilder.DefaultDestructurers, options.Destructurers);
        Assert.Equal(DestructuringOptionsBuilder.IgnoreStackTraceAndTargetSiteExceptionFilter, options.Filter);
    }

    [Fact]
    public void WithExceptionDetails_AllExplicitOptions()
    {
        // language=json
        const string json = """
            {
              "Name": "WithExceptionDetails",
              "Args": {
                "defaultDestructurers": false,
                "destructurers": [
                  { "type": "Serilog.Exceptions.Destructurers.ExceptionDestructurer, Serilog.Exceptions" },
                  { "type": "Serilog.Exceptions.Destructurers.ArgumentExceptionDestructurer, Serilog.Exceptions" },
                ],
                "defaultFilters": false,
                "filters": [
                  {
                    "type": "Serilog.Exceptions.Filters.IgnorePropertyByNameExceptionFilter, Serilog.Exceptions",
                    "propertiesToIgnore": [ "ExplicitPropertyName" ]
                  }
                ],
                "rootName": "ExplicitRootName",
                "destructuringDepth": 123,
                "disableReflectionBasedDestructurer": true
              }
            }
            """;
        var options = ConfigureJson(json);

        Assert.Equal("ExplicitRootName", options.RootName);
        Assert.Equal(123, options.DestructuringDepth);
        Assert.True(options.DisableReflectionBasedDestructurer);
        Assert.Equal([new ExceptionDestructurer(), new ArgumentExceptionDestructurer()], options.Destructurers, SameTypeComparer.Instance);
        var filter = Assert.IsType<IgnorePropertyByNameExceptionFilter>(options.Filter);
        Assert.True(filter.ShouldPropertyBeFiltered(new InvalidOperationException(), "ExplicitPropertyName", null));
    }

    [Fact]
    public void WithExceptionDetails_WithoutDestructurers()
    {
        // language=json
        const string json = """
            {
              "Name": "WithExceptionDetails",
              "Args": {
                "defaultDestructurers": false
              }
            }
            """;
        var options = ConfigureJson(json);

        Assert.Empty(options.Destructurers);
    }

    [Fact]
    public void WithExceptionDetails_DefaultDestructurersWithRpcException()
    {
        // language=json
        const string json = """
            {
              "Name": "WithExceptionDetails",
              "Args": {
                "destructurers": [ { "type": "Serilog.Exceptions.Grpc.Destructurers.RpcExceptionDestructurer, Serilog.Exceptions.Grpc" } ]
              }
            }
            """;
        var options = ConfigureJson(json);

        Assert.Equal([.. DestructuringOptionsBuilder.DefaultDestructurers, new RpcExceptionDestructurer()], options.Destructurers, SameTypeComparer.Instance);
    }

    [Fact]
    public void WithExceptionDetails_OnlyRpcExceptionDestructurer()
    {
        // language=json
        const string json = """
            {
              "Name": "WithExceptionDetails",
              "Args": {
                "defaultDestructurers": false,
                "destructurers": [ { "type": "Serilog.Exceptions.Grpc.Destructurers.RpcExceptionDestructurer, Serilog.Exceptions.Grpc" } ]
              }
            }
            """;
        var options = ConfigureJson(json);

        _ = Assert.IsType<RpcExceptionDestructurer>(Assert.Single(options.Destructurers));
    }

    [Fact]
    public void WithExceptionDetails_WithoutFilters()
    {
        // language=json
        const string json = """
            {
              "Name": "WithExceptionDetails",
              "Args": {
                "defaultFilters": false
              }
            }
            """;
        var options = ConfigureJson(json);

        Assert.Null(options.Filter);
    }

    [Fact]
    public void WithExceptionDetails_WithSingleFilter()
    {
        // language=json
        const string json = """
            {
              "Name": "WithExceptionDetails",
              "Args": {
                "defaultFilters": false,
                "filters": [
                  {
                    "type": "Serilog.Exceptions.Test.LoggerEnrichmentConfigurationExtensionsTests+TestFilter, Serilog.Exceptions.Test",
                    "name": "TestName"
                  }
                ]
              }
            }
            """;
        var options = ConfigureJson(json);

        var filter = Assert.IsType<TestFilter>(options.Filter);
        Assert.Equal("TestName", filter.Name);
    }

    [Fact]
    public void WithExceptionDetails_DefaultFilterWithExplicitFilters()
    {
        // language=json
        const string json = """
            {
              "Name": "WithExceptionDetails",
              "Args": {
                "filters": [
                  {
                    "type": "Serilog.Exceptions.Test.LoggerEnrichmentConfigurationExtensionsTests+TestFilter, Serilog.Exceptions.Test",
                    "name": "FirstExplicit"
                  },
                  {
                    "type": "Serilog.Exceptions.Test.LoggerEnrichmentConfigurationExtensionsTests+TestFilter, Serilog.Exceptions.Test",
                    "name": "SecondExplicit"
                  }
                ]
              }
            }
            """;
        var options = ConfigureJson(json);

        var filter = Assert.IsType<CompositeExceptionPropertyFilter>(options.Filter);
        var filters = typeof(CompositeExceptionPropertyFilter).GetField("filters", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(filter) as IExceptionPropertyFilter[];
        Assert.NotNull(filters);
        Assert.Collection(filters,
            first => Assert.Equal(DestructuringOptionsBuilder.IgnoreStackTraceAndTargetSiteExceptionFilter, first),
            second => Assert.Equal("FirstExplicit", Assert.IsType<TestFilter>(second).Name),
            third => Assert.Equal("SecondExplicit", Assert.IsType<TestFilter>(third).Name));
    }

    [Fact]
    public void WithExceptionDetails_WithMultipleFilters()
    {
        // language=json
        const string json = """
            {
              "Name": "WithExceptionDetails",
              "Args": {
                "defaultFilters": false,
                "filters": [
                  {
                    "type": "Serilog.Exceptions.Test.LoggerEnrichmentConfigurationExtensionsTests+TestFilter, Serilog.Exceptions.Test",
                    "name": "First"
                  },
                  {
                    "type": "Serilog.Exceptions.Test.LoggerEnrichmentConfigurationExtensionsTests+TestFilter, Serilog.Exceptions.Test",
                    "name": "Second"
                  }
                ]
              }
            }
            """;
        var options = ConfigureJson(json);

        var filter = Assert.IsType<CompositeExceptionPropertyFilter>(options.Filter);
        var filters = typeof(CompositeExceptionPropertyFilter).GetField("filters", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(filter) as IExceptionPropertyFilter[];
        Assert.NotNull(filters);
        Assert.Collection(filters,
            first => Assert.Equal("First", Assert.IsType<TestFilter>(first).Name),
            second => Assert.Equal("Second", Assert.IsType<TestFilter>(second).Name));
    }

    [Fact]
    public void WithExceptionDetails_WithRootName()
    {
        // language=json
        const string json = """
            {
              "Name": "WithExceptionDetails",
              "Args": {
                "rootName": "ExplicitRootName",
              }
            }
            """;
        var options = ConfigureJson(json);

        Assert.Equal("ExplicitRootName", options.RootName);
    }

    [Fact]
    public void WithExceptionDetails_WithDestructuringDepth()
    {
        // language=json
        const string json = """
            {
              "Name": "WithExceptionDetails",
              "Args": {
                "destructuringDepth": 123,
              }
            }
            """;
        var options = ConfigureJson(json);

        Assert.Equal(123, options.DestructuringDepth);
    }

    [Fact]
    public void WithExceptionDetails_WithoutReflectionBasedDestructurer()
    {
        // language=json
        const string json = """
            {
              "Name": "WithExceptionDetails",
              "Args": {
                "disableReflectionBasedDestructurer": true,
              }
            }
            """;
        var options = ConfigureJson(json);

        Assert.True(options.DisableReflectionBasedDestructurer);
    }

    private static IDestructuringOptions ConfigureJson(string json)
    {
        using var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, leaveOpen: true))
        {
            // language=json
            writer.Write($$"""
            {
              "Serilog": {
                "Using": [ "Serilog.Exceptions", "Serilog.Exceptions.Test" ],
                "Enrich": [{
                    "Name": "CaptureOptions",
                    "Args": {
                        "configureEnricher": [ {{json}} ]
                    }
                }],
              }
            }
            """);
        }
        stream.Position = 0;
        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();

        CaptureOptionsExtensions.ClearLastCaptured();
        _ = new LoggerConfiguration().ReadFrom.Configuration(config);
        var captured = CaptureOptionsExtensions.LastCaptured;
        CaptureOptionsExtensions.ClearLastCaptured();
        Assert.NotNull(captured);
        return captured;
    }

    private sealed class SameTypeComparer : IEqualityComparer<IExceptionDestructurer>
    {
        public static readonly SameTypeComparer Instance = new();

        public bool Equals(IExceptionDestructurer? x, IExceptionDestructurer? y) => x?.GetType() == y?.GetType();
        public int GetHashCode([DisallowNull] IExceptionDestructurer obj) => obj.GetType().GetHashCode();
    }

    public sealed class TestFilter(string name) : IExceptionPropertyFilter
    {
        public string Name { get; } = name;

        public bool ShouldPropertyBeFiltered(Exception exception, string propertyName, object? value) => false;
    }
}

public static class CaptureOptionsExtensions
{
    internal static IDestructuringOptions? LastCaptured { get; private set; }

    internal static void ClearLastCaptured() => LastCaptured = null;

    public static LoggerConfiguration CaptureOptions(this LoggerEnrichmentConfiguration loggerEnrichmentConfiguration, Action<LoggerEnrichmentConfiguration> configureEnricher) =>
        LoggerEnrichmentConfiguration.Wrap(loggerEnrichmentConfiguration, e =>
        {
            var destructuringOptions = typeof(ExceptionEnricher).GetField("destructuringOptions", BindingFlags.NonPublic | BindingFlags.Instance);
            LastCaptured = destructuringOptions?.GetValue(e) as IDestructuringOptions;
            return e;
        }, configureEnricher);
}
