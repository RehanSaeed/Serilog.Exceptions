namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections.Generic;

    internal class ExceptionPropertiesBag : IExceptionPropertiesBag
    {
        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();
        private bool resultsCollected = false;

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
            if (this.resultsCollected)
            {
                throw new InvalidOperationException($"Cannot add exception property '{key}' to bag, after results were already collected");
            }

            this.properties.Add(key, value);
        }
    }
}