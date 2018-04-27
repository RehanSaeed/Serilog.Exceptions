namespace Serilog.Exceptions.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// List based dictionary
    /// </summary>
    public class ListBasedDictionary : IReadOnlyDictionary<string, object>
    {
        private List<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>(10);

        /// <inheritdoc />
        public IEnumerable<string> Keys => this.list.Select(x => x.Key);

        /// <inheritdoc />
        public IEnumerable<object> Values => this.list.Select(x => x.Value);

        /// <summary>
        /// Count
        /// </summary>
        public int Count => this.list.Count;

        /// <summary>
        /// Indexer
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>Returns</returns>
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

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="item">The item</param>
        public void Add(KeyValuePair<string, object> item)
        {
            this.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Clear
        /// </summary>
        public void Clear()
        {
            this.list.Clear();
        }

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        public void Add(string key, object value)
        {
            this.list.Add(new KeyValuePair<string, object>(key, value));
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Contains key
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>bol</returns>
        public bool ContainsKey(string key)
        {
            return this.list.Any(x => x.Key == key);
        }

        /// <inheritdoc />
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
    }
}