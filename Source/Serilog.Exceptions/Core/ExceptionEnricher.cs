namespace Serilog.Exceptions.Core
{
    using System;
    using System.Collections.Generic;
    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Exceptions.Destructurers;

    /// <summary>
    /// Enrich a <see cref="LogEvent"/> with details about an <see cref="LogEvent.Exception"/> if present.
    /// </summary>
    public sealed class ExceptionEnricher : ILogEventEnricher
    {
        private readonly IExceptionDestructurer reflectionBasedDestructurer;
        private readonly Dictionary<Type, IExceptionDestructurer> destructurers;
        private readonly IDestructuringOptions destructuringOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEnricher"/> class.
        /// </summary>
        public ExceptionEnricher()
            : this(new DestructuringOptionsBuilder().WithDefaultDestructurers())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEnricher"/> class.
        /// </summary>
        /// <param name="destructuringOptions">The destructuring options, cannot be null.</param>
        public ExceptionEnricher(IDestructuringOptions destructuringOptions)
        {
            this.destructuringOptions = destructuringOptions ?? throw new ArgumentNullException(nameof(destructuringOptions));
            this.reflectionBasedDestructurer = new ReflectionBasedDestructurer(destructuringOptions.DestructuringDepth);

            this.destructurers = new Dictionary<Type, IExceptionDestructurer>();
            foreach (var destructurer in this.destructuringOptions.Destructurers)
            {
                foreach (var targetType in destructurer.TargetTypes)
                {
                    this.destructurers.Add(targetType, destructurer);
                }
            }
        }

        /// <summary>
        /// Enriches <paramref name="logEvent"/> with a destructured exception's properties. If the exception is not
        /// present, no action is taken.
        /// </summary>
        /// <param name="logEvent">Log event that will be enriched.</param>
        /// <param name="propertyFactory">The property factory.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(logEvent);
            ArgumentNullException.ThrowIfNull(propertyFactory);
#else
            if (logEvent is null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            if (propertyFactory is null)
            {
                throw new ArgumentNullException(nameof(propertyFactory));
            }
#endif

            if (logEvent.Exception is not null)
            {
                var dataDictionary = this.DestructureException(logEvent.Exception);

                if (dataDictionary is not null)
                {
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                        this.destructuringOptions.RootName,
                        dataDictionary,
                        true));
                }
            }
        }

        private IReadOnlyDictionary<string, object?>? DestructureException(Exception exception)
        {
            var exceptionType = exception.GetType();

            if (this.destructurers.ContainsKey(exceptionType))
            {
                var data = new ExceptionPropertiesBag(exception, this.destructuringOptions.Filter);

                var destructurer = this.destructurers[exceptionType];
                destructurer.Destructure(exception, data, this.DestructureException);

                return data.GetResultDictionary();
            }
            else if (!this.destructuringOptions.DisableReflectionBasedDestructurer)
            {
                var data = new ExceptionPropertiesBag(exception, this.destructuringOptions.Filter);

                this.reflectionBasedDestructurer.Destructure(exception, data, this.DestructureException);

                return data.GetResultDictionary();
            }

            return null;
        }
    }
}
