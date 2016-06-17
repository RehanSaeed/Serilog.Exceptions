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
        public static readonly IExceptionDestructurer[] DefaultDestructurers =
        {
            new ExceptionDestructurer(),
            new ArgumentExceptionDestructurer(),
            new ArgumentOutOfRangeExceptionDestructurer(),
            new AggregateExceptionDestructurer(),
            new ReflectionTypeLoadExceptionDestructurer(),
            new SqlExceptionDestructurer()
        };

        public static readonly IExceptionDestructurer ReflectionBasedDestructurer = new ReflectionBasedDestructurer();

        private readonly Dictionary<Type, IExceptionDestructurer> destructurers;

        public ExceptionEnricher()
            : this(DefaultDestructurers)
        {
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

        private Dictionary<string, object> DestructureException(Exception exception)
        {
            var data = new Dictionary<string, object>();

            var exceptionType = exception.GetType();

            if (this.destructurers.ContainsKey(exceptionType))
            {
                var destructurer = this.destructurers[exceptionType];
                destructurer.Destructure(exception, data, this.DestructureException);
            }
            else
            {
                ReflectionBasedDestructurer.Destructure(exception, data, this.DestructureException);
            }

            return data;
        }
    }
}