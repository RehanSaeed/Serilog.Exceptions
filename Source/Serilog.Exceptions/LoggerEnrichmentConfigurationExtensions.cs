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
        /// exception's public properties.
        /// </summary>
        /// <param name="loggerEnrichmentConfiguration">The enrichment configuration</param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static Serilog.LoggerConfiguration WithExceptionDetails(
            this LoggerEnrichmentConfiguration loggerEnrichmentConfiguration)
        {
            return loggerEnrichmentConfiguration.With(new ExceptionEnricher());
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

        public static Serilog.LoggerConfiguration WithProperties(
            this LoggerEnrichmentConfiguration loggerEnrichmentConfiguration,
            IDictionary<string, object> properties)
        {
            if (loggerEnrichmentConfiguration == null)
            {
                throw new ArgumentNullException(nameof(loggerEnrichmentConfiguration));
            }

            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            if (properties.Count == 0)
            {
                throw new ArgumentException("Logger properties count cannot be zero.", nameof(properties));
            }

            Serilog.LoggerConfiguration loggerConfiguration = null;

            foreach (var property in properties)
            {
                loggerConfiguration = loggerEnrichmentConfiguration.WithProperty(property.Key, property.Value);
            }

            return loggerConfiguration;
        }

        public static Serilog.LoggerConfiguration WithLazyProperties(
            this LoggerEnrichmentConfiguration loggerEnrichmentConfiguration,
            IDictionary<string, Func<object>> properties)
        {
            if (loggerEnrichmentConfiguration == null)
            {
                throw new ArgumentNullException(nameof(loggerEnrichmentConfiguration));
            }

            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            if (properties.Count == 0)
            {
                throw new ArgumentException("Logger properties count cannot be zero.", nameof(properties));
            }

            Serilog.LoggerConfiguration loggerConfiguration = null;

            foreach (var property in properties)
            {
                loggerConfiguration = loggerEnrichmentConfiguration.WithProperty(property.Key, property.Value());
            }

            return loggerConfiguration;
        }
    }
}
