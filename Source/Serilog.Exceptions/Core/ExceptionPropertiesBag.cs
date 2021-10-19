namespace Serilog.Exceptions.Core
{
    using System;
    using System.Collections.Generic;
    using Serilog.Exceptions.Filters;

    /// <inheritdoc />
    internal class ExceptionPropertiesBag : IExceptionPropertiesBag
    {
        /// <summary>
        /// In theory there should not be any properties with same names passed
        /// but just as a defensive and robustness measure, we allow small amount
        /// so that library can keep working in case of minor error (or unexpected
        /// scenario) on the properties provider side.
        /// </summary>
        private const int AcceptableNumberOfSameNameProperties = 5;
        private readonly Exception exception;
        private readonly IExceptionPropertyFilter? filter;
        private readonly Dictionary<string, object?> properties = new();

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
        public ExceptionPropertiesBag(Exception exception, IExceptionPropertyFilter? filter = null)
        {
            this.exception = exception ?? throw new ArgumentNullException(nameof(exception));
            this.filter = filter;
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, object?> GetResultDictionary()
        {
            this.resultsCollected = true;
            return this.properties;
        }

        /// <inheritdoc />
        public void AddProperty(string key, object? value)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(key);
#else
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }
#endif

            if (this.resultsCollected)
            {
                throw new InvalidOperationException($"Cannot add exception property '{key}' to bag, after results were already collected");
            }

            if (this.filter is not null)
            {
                if (this.filter.ShouldPropertyBeFiltered(this.exception, key, value))
                {
                    return;
                }
            }

            this.AddPairToProperties(key, value);
        }

        /// <inheritdoc />
        public bool ContainsProperty(string key) => this.properties.ContainsKey(key);

        private static string GetReplacementKey(string key) => key + "$";

        /// <summary>
        /// We want to be as robust as possible
        /// so even in case of multiple properties with the same name
        /// we want to at least try carrying on and keep working.
        /// </summary>
        private void AddPairToProperties(string key, object? value)
        {
#if NET5_0_OR_GREATER
            var i = 0;
            while (!this.properties.TryAdd(key, value) && i < AcceptableNumberOfSameNameProperties)
            {
                key = GetReplacementKey(key);
                i++;
            }
#else
            key = this.MakeSureKeyIsUnique(key);

            this.properties.Add(key, value);
#endif
        }

#if !NET5_0_OR_GREATER
        private string MakeSureKeyIsUnique(string key)
        {
            var i = 0;
            while (this.properties.ContainsKey(key) && i < AcceptableNumberOfSameNameProperties)
            {
                key = GetReplacementKey(key);
                i++;
            }

            return key;
        }
#endif
    }
}
