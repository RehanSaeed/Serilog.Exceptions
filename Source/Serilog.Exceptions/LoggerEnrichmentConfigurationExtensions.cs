namespace Serilog.Exceptions
{
    using System;
    using Serilog.Configuration;
    using Serilog.Core;
    using Serilog.Exceptions.Core;

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
    }
}
