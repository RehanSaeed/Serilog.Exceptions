namespace Serilog.Exceptions.Core
{
    using System;
    using System.Collections.Generic;

    internal class ExceptionPropertiesBag : IExceptionPropertiesBag
    {
        private readonly Type exceptionType;
        private readonly IExceptionPropertyFilter filter;
        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();
        private bool resultsCollected = false;

        public ExceptionPropertiesBag(Type exceptionType, IExceptionPropertyFilter filter = null)
        {
            this.exceptionType = exceptionType;
            this.filter = filter;
        }

        public IReadOnlyDictionary<string, object> ResultDictionary
        {
            get
            {
                this.resultsCollected = true;
                return this.properties;
            }
        }

        public void AddProperty(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key), "Cannot add exception property without a key");
            }

            if (this.resultsCollected)
            {
                throw new InvalidOperationException($"Cannot add exception property '{key}' to bag, after results were already collected");
            }

            if (this.filter != null)
            {
                if (this.filter.ShouldPropertyBeFiltered(this.exceptionType, key, value))
                {
                    return;
                }
            }

            this.properties.Add(key, value);
        }
    }
}