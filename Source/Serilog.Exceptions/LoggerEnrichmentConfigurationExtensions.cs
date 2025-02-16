namespace Serilog.Exceptions;

using Serilog.Configuration;
using Serilog.Core;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Destructurers;
using Serilog.Exceptions.Filters;

/// <summary>
/// Serilog logger enrichment extension methods.
/// </summary>
public static class LoggerEnrichmentConfigurationExtensions
{
    /// <summary>
    /// Enrich logger output with a destructured object containing exception's public properties. Default
    /// destructurers are registered. <see cref="Exception.StackTrace"/> and Exception.TargetSite
    /// are omitted by the destructuring process because Serilog already attaches them to log event.
    /// </summary>
    /// <param name="loggerEnrichmentConfiguration">The enrichment configuration.</param>
    /// <returns>Configuration object allowing method chaining.</returns>
    public static LoggerConfiguration WithExceptionDetails(
        this LoggerEnrichmentConfiguration loggerEnrichmentConfiguration)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(loggerEnrichmentConfiguration);
#else
        if (loggerEnrichmentConfiguration is null)
        {
            throw new ArgumentNullException(nameof(loggerEnrichmentConfiguration));
        }
#endif

        var options = new DestructuringOptionsBuilder()
            .WithDefaultDestructurers()
            .WithIgnoreStackTraceAndTargetSiteExceptionFilter();
        var logEventEnricher = new ExceptionEnricher(options);
        return loggerEnrichmentConfiguration.With(logEventEnricher);
    }

    /// <summary>
    /// Enrich logger output with a destuctured object containing exception's public properties.
    /// </summary>
    /// <param name="loggerEnrichmentConfiguration">The enrichment configuration.</param>
    /// <param name="destructuringOptions">
    /// Options that will influence the process of destructuring exception's properties into result object.
    /// </param>
    /// <returns>Configuration object allowing method chaining.</returns>
    public static LoggerConfiguration WithExceptionDetails(
        this LoggerEnrichmentConfiguration loggerEnrichmentConfiguration,
        IDestructuringOptions destructuringOptions)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(loggerEnrichmentConfiguration);
#else
        if (loggerEnrichmentConfiguration is null)
        {
            throw new ArgumentNullException(nameof(loggerEnrichmentConfiguration));
        }
#endif

        ILogEventEnricher enricher = new ExceptionEnricher(destructuringOptions);
        return loggerEnrichmentConfiguration.With(enricher);
    }

    /// <summary>
    /// Enrich logger output with a destuctured object containing exception's public properties.
    /// </summary>
    /// <param name="loggerEnrichmentConfiguration">The enrichment configuration.</param>
    /// <param name="defaultDestructurers">Add destructurers for a known set of exceptions from standard library.</param>
    /// <param name="destructurers">A collection of destructurers that will be used to handle exception.</param>
    /// <param name="defaultFilters">Add default filters.</param>
    /// <param name="filters">Global filters, that will be applied to every destructured property just before it is added to the result.</param>
    /// <param name="rootName">The name of the property which value will be filled with destructured exception.</param>
    /// <param name="destructuringDepth">The maximum depth of destructuring to which reflection destructurer will proceed.</param>
    /// <param name="disableReflectionBasedDestructurer">Disable the reflection based destructurer.</param>
    /// <returns>Configuration object allowing method chaining.</returns>
    public static LoggerConfiguration WithExceptionDetails(
        this LoggerEnrichmentConfiguration loggerEnrichmentConfiguration,
        bool defaultDestructurers = true,
        IExceptionDestructurer[]? destructurers = null,
        bool defaultFilters = true,
        IExceptionPropertyFilter[]? filters = null,
        string? rootName = null,
        int? destructuringDepth = null,
        bool disableReflectionBasedDestructurer = false)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(loggerEnrichmentConfiguration);
#else
        if (loggerEnrichmentConfiguration is null)
        {
            throw new ArgumentNullException(nameof(loggerEnrichmentConfiguration));
        }
#endif

        var builder = new DestructuringOptionsBuilder();

        if (defaultDestructurers)
        {
            builder.WithDefaultDestructurers();
        }

        if (destructurers is not null && destructurers.Length > 0)
        {
            builder.WithDestructurers(destructurers);
        }

        if (filters is not null && filters.Length > 0)
        {
            if (defaultFilters)
            {
                var composite = new IExceptionPropertyFilter[filters.Length + 1];
                composite[0] = DestructuringOptionsBuilder.IgnoreStackTraceAndTargetSiteExceptionFilter;
                Array.Copy(filters, 0, composite, 1, filters.Length);
                builder.WithFilter(new CompositeExceptionPropertyFilter(composite));
            }
            else if (filters.Length == 1)
            {
                builder.WithFilter(filters[0]);
            }
            else
            {
                builder.WithFilter(new CompositeExceptionPropertyFilter(filters));
            }
        }
        else if (defaultFilters)
        {
            builder.WithIgnoreStackTraceAndTargetSiteExceptionFilter();
        }

        if (!string.IsNullOrEmpty(rootName))
        {
            builder.WithRootName(rootName!);
        }

        if (destructuringDepth.HasValue)
        {
            builder.WithDestructuringDepth(destructuringDepth.Value);
        }

        if (disableReflectionBasedDestructurer)
        {
            builder.WithoutReflectionBasedDestructurer();
        }

        var enricher = new ExceptionEnricher(builder);
        return loggerEnrichmentConfiguration.With(enricher);
    }
}
