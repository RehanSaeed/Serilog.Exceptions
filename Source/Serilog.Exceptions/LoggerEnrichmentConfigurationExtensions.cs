namespace Serilog.Exceptions
{
    using System;
    using System.Collections.Generic;
    using Serilog.Configuration;
    using Serilog.Core;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Destructurers;
    using Serilog.Exceptions.Filters;

    public static class LoggerEnrichmentConfigurationExtensions
    {
        /// <summary>
        /// Enrich logger output with a destuctured object containing
        /// exception's public properties. Default destructurers are registered.
        /// <see cref="Exception.StackTrace"/> and <see cref="Exception.TargetSite"/> are omitted
        /// by the destructuring process because Serilog already attaches them to log event.
        /// </summary>
        /// <param name="loggerEnrichmentConfiguration">The enrichment configuration</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static Serilog.LoggerConfiguration WithExceptionDetails(
            this LoggerEnrichmentConfiguration loggerEnrichmentConfiguration)
        {
            var options = new DestructuringOptionsBuilder()
                .WithDefaultDestructurers()
                .WithIgnoreStackTraceAndTargetSiteExceptionFilter();
            var logEventEnricher = new ExceptionEnricher(options);
            return loggerEnrichmentConfiguration.With(logEventEnricher);
        }

        /// <summary>
        /// Enrich logger output with a destuctured object containing
        /// exception's public properties.
        /// </summary>
        /// <param name="loggerEnrichmentConfiguration">The enrichment configuration</param>
        /// <param name="destructurers">
        /// Destructurers that will be used to destructure captured exceptions.
        /// </param>
        /// <returns>Configuration object allowing method chaining.</returns>
        [Obsolete("Use new, fluent API based on the DestructuringOptionsBuilder. To specify destructurers, call WithDestructurers method.")]
        public static Serilog.LoggerConfiguration WithExceptionDetails(
            this LoggerEnrichmentConfiguration loggerEnrichmentConfiguration,
            params IExceptionDestructurer[] destructurers)
        {
            return loggerEnrichmentConfiguration.With(new ExceptionEnricher(destructurers));
        }

        /// <summary>
        /// Enrich logger output with a destuctured object containing
        /// exception's public properties.
        /// </summary>
        /// <param name="loggerEnrichmentConfiguration">The enrichment configuration</param>
        /// <param name="destructurers">
        /// Destructurers that will be used to destructure captured exceptions.
        /// </param>
        /// <returns>Configuration object allowing method chaining.</returns>
        [Obsolete("Use new, fluent API based on the DestructuringOptionsBuilder. To specify destructurers, call WithDestructurers method.")]
        public static Serilog.LoggerConfiguration WithExceptionDetails(
            this LoggerEnrichmentConfiguration loggerEnrichmentConfiguration,
            IEnumerable<IExceptionDestructurer> destructurers)
        {
            ILogEventEnricher enricher = new ExceptionEnricher(new DestructuringOptionsBuilder().WithDestructurers(destructurers));
            return loggerEnrichmentConfiguration.With(enricher);
        }

        /// <summary>
        /// Enrich logger output with a destuctured object containing
        /// exception's public properties.
        /// </summary>
        /// <param name="loggerEnrichmentConfiguration">The enrichment configuration</param>
        /// <param name="destructuringOptions">
        /// Options that will influence the process of destructuring
        /// exception's properties into result object.
        /// </param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static Serilog.LoggerConfiguration WithExceptionDetails(
            this LoggerEnrichmentConfiguration loggerEnrichmentConfiguration,
            IDestructuringOptions destructuringOptions)
        {
            ILogEventEnricher enricher = new ExceptionEnricher(destructuringOptions);
            return loggerEnrichmentConfiguration.With(enricher);
        }
    }
}
