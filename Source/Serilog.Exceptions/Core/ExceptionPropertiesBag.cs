namespace Serilog.Exceptions.Core
{
    using System;
    using System.Collections.Generic;
    using Serilog.Exceptions.Filters;

    /// <inheritdoc />
    internal class ExceptionPropertiesBag : IExceptionPropertiesBag
    {
        private readonly Exception exception;
        private readonly IExceptionPropertyFilter filter;
        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        /// <summary>
        /// We keep a note on whether the results were collected to be sure that after that there are no changes. This
        /// is the application of fail-fast principle.
        /// </summary>
        private bool resultsCollected;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionPropertiesBag"/> class.
        /// </summary>
        /// <param name="exception">The exception which properties will be added to the bag.</param>
        /// <param name="filter">Filter that should be applied to each property just before adding it to the bag.</param>
        public ExceptionPropertiesBag(Exception exception, IExceptionPropertyFilter filter = null)
        {
            this.exception = exception ?? throw new ArgumentNullException(nameof(exception));
            this.filter = filter;
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, object> GetResultDictionary()
        {
            this.resultsCollected = true;
            return this.properties;
        }

        /// <inheritdoc />
        public void AddProperty(string key, object value)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (this.resultsCollected)
            {
                throw new InvalidOperationException($"Cannot add exception property '{key}' to bag, after results were already collected");
            }

            if (this.filter is object)
            {
                if (this.filter.ShouldPropertyBeFiltered(this.exception, key, value))
                {
                    return;
                }
            }

            this.properties.Add(key, value);
        }

        /// <inheritdoc />
        public bool ContainsProperty(string key) => this.properties.ContainsKey(key);
    }
}
