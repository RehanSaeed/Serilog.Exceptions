using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Filters;

namespace Serilog.Exceptions.Benchmark
{
    internal class FastExceptionPropertiesBag : IExceptionPropertiesBag
    {
        private readonly Exception exception;
        private readonly IExceptionPropertyFilter filter;
        private readonly ListBasedDictionary properties = new ListBasedDictionary();

        // We keep a note on whether the results were collected to be sure that
        // after that there are no changes. This is the application of fail-fast principle.
        private bool resultsCollected = false;

        public FastExceptionPropertiesBag(Exception exception, IExceptionPropertyFilter filter = null)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception),
                    $"Cannot create {nameof(ExceptionPropertiesBag)} for null exception");
            }

            this.exception = exception;
            this.filter = filter;
        }

        public IReadOnlyDictionary<string, object> GetResultDictionary()
        {
            this.resultsCollected = true;
            return this.properties;
        }

        public void AddProperty(string key, object value)
        {
            if (key == null)
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

        public bool ContainsProperty(string key)
        {
            return this.properties.ContainsKey(key);
        }
    }

    internal class ListBasedDictionary : IReadOnlyDictionary<string, object>
    {
        private List<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>(10);

        public void Add(string key, object value)
        {
            this.list.Add(new KeyValuePair<string, object>(key, value));
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => this.list.Count;

        public bool ContainsKey(string key)
        {
            return this.list.Any(x => x.Key == key);
        }

        public bool TryGetValue(string key, out object value)
        {
            foreach (KeyValuePair<string, object> keyValuePair in this.list)
            {
                if (keyValuePair.Key == key)
                {
                    value = keyValuePair.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        public object this[string key]
        {
            get
            {
                foreach (KeyValuePair<string, object> keyValuePair in this.list)
                {
                    if (keyValuePair.Key == key)
                    {
                        return keyValuePair.Value;
                    }
                }

                return null;
            }
        }

        public IEnumerable<string> Keys => this.list.Select(x => x.Key);
        public IEnumerable<object> Values => this.list.Select(x => x.Value);
    }
}