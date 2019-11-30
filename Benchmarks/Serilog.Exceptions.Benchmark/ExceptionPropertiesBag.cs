namespace Serilog.Exceptions.Benchmark
{
    using System;
    using System.Collections.Generic;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Filters;

    internal class ExceptionPropertiesBag : IExceptionPropertiesBag
    {
        private readonly Exception exception;
        private readonly IExceptionPropertyFilter filter;
        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        // We keep a note on whether the results were collected to be sure that
        // after that there are no changes. This is the application of fail-fast principle.
        private bool resultsCollected = false;

        public ExceptionPropertiesBag(Exception exception, IExceptionPropertyFilter filter = null)
        {
            this.exception = exception ?? throw new ArgumentNullException(
                nameof(exception),
                $"Cannot create {nameof(ExceptionPropertiesBag)} for null exception");
            this.filter = filter;
        }

        public IReadOnlyDictionary<string, object> GetResultDictionary()
        {
            this.resultsCollected = true;
            return this.properties;
        }

        public void AddProperty(string key, object value)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key), "Cannot add exception property without a key");
            }

            if (this.resultsCollected)
            {
                throw new InvalidOperationException(
                    $"Cannot add exception property '{key}' to bag, after results were already collected");
            }

            if (this.filter != null)
            {
                if (this.filter.ShouldPropertyBeFiltered(this.exception, key, value))
                {
                    return;
                }
            }

            this.properties.Add(key, value);
        }

        public bool ContainsProperty(string key) => this.properties.ContainsKey(key);
    }
}