namespace Serilog.Exceptions.Destructurers
{
    using System.Collections.Generic;

    public interface IExceptionPropertiesBag
    {
        IReadOnlyDictionary<string, object> ResultDictionary { get; }

        void AddProperty(string key, object value);
    }

    internal class ExceptionPropertiesBag : IExceptionPropertiesBag
    {
        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        public IReadOnlyDictionary<string, object> ResultDictionary => this.properties;

        public void AddProperty(string key, object value)
        {
            this.properties.Add(key, value);
        }
    }
}
