namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;
    using Serilog.Core;
    using Serilog.Events;

    /// <summary>
    /// Enrich a <see cref="LogEvent"/> with details about an <see cref="LogEvent.Exception"/> if present.
    /// https://groups.google.com/forum/#!searchin/getseq/enhance$20exception/getseq/rsAL4u3JpLM/PrszbPbtEb0J
    /// </summary>
    public sealed class ExceptionEnricher : ILogEventEnricher
    {
        private static IExceptionDestructurer reflectionBasedDestructurer;
        private readonly Dictionary<Type, IExceptionDestructurer> destructurers;

        public ExceptionEnricher()
            : this(GetDefaultDestructurers(new List<string>()))
        {
        }

        public ExceptionEnricher(List<string> ignoredProperties)
            : this(GetDefaultDestructurers(ignoredProperties))
        {
            reflectionBasedDestructurer = new ReflectionBasedDestructurer(ignoredProperties);
        }

        public ExceptionEnricher(params IExceptionDestructurer[] destructurers)
            : this((IEnumerable<IExceptionDestructurer>)destructurers)
        {
        }

        public ExceptionEnricher(IEnumerable<IExceptionDestructurer> destructurers)
        {
            this.destructurers = new Dictionary<Type, IExceptionDestructurer>();
            foreach (var destructurer in destructurers)
            {
                foreach (var targetType in destructurer.TargetTypes)
                {
                    this.destructurers.Add(targetType, destructurer);
                }
            }
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Exception != null)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                    "ExceptionDetail",
                    this.DestructureException(logEvent.Exception),
                    true));
            }
        }

        private static IExceptionDestructurer[] GetDefaultDestructurers(List<string> ignoredProperties)
        {
            var list = new List<IExceptionDestructurer>
            {
                new ExceptionDestructurer(ignoredProperties),
                new ArgumentExceptionDestructurer(ignoredProperties),
                new ArgumentOutOfRangeExceptionDestructurer(ignoredProperties),
                new AggregateExceptionDestructurer(ignoredProperties),
                new ReflectionTypeLoadExceptionDestructurer(ignoredProperties)
            };
            return list.ToArray();
        }

        private Dictionary<string, object> DestructureException(Exception exception)
        {
            var data = new Dictionary<string, object>();

            var exceptionType = exception.GetType();

            if (this.destructurers.ContainsKey(exceptionType))
            {
                var destructurer = this.destructurers[exceptionType];
                destructurer.Destructure(exception, data,  this.DestructureException);
            }
            else
            {
                reflectionBasedDestructurer.Destructure(exception, data, this.DestructureException);
            }

            return data;
        }
    }
}