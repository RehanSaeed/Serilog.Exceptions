namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class ReflectionBasedDestructurer : IExceptionDestructurer
    {
        private const int MaxRecursiveLevel = 10;

        public Type[] TargetTypes
        {
            get { return new Type[] { typeof(Exception) }; }
        }

        public void Destructure(
            Exception exception,
            IDictionary<string, object> data,
            Func<Exception, IDictionary<string, object>> destructureException)
        {
            foreach (var p in this.DestructureObject(exception, exception.GetType(), 0))
            {
                data.Add(p.Key, p.Value);
            }
        }

        private object DestructureValue(object value, int level)
        {
            if (value == null)
            {
                return null;
            }

            var valueType = value.GetType();
            var valueTypeInfo = valueType.GetTypeInfo();

            if (valueTypeInfo.IsSubclassOf(typeof(MemberInfo)))
            {
                return value;
            }

            if (valueType.GetTypeCode() != TypeCode.Object || valueTypeInfo.IsValueType)
            {
                return value;
            }

            if (level >= MaxRecursiveLevel)
            {
                return value;
            }

            if (typeof(IDictionary).GetTypeInfo().IsAssignableFrom(valueTypeInfo))
            {
                return ((IDictionary)value)
                    .Cast<DictionaryEntry>()
                    .Where(e => e.Key is string)
                    .ToDictionary(e => (string)e.Key, e => this.DestructureValue(e.Value, level + 1));
            }

            if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(valueTypeInfo))
            {
                return ((IEnumerable)value)
                    .Cast<object>()
                    .Select(o => this.DestructureValue(o, level + 1))
                    .ToList();
            }

            return this.DestructureObject(value, valueType, level);
        }

        private IDictionary<string, object> DestructureObject(object value, Type valueType, int level)
        {
            var values = valueType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToDictionary(
                    p => p.Name,
                    p => this.DestructureValue(p.GetValue(value),
                    level + 1));

            values.Add("Type", valueType);

            return values;
        }
    }
}
