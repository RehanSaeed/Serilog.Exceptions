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
            foreach (var p in this.DestructureObject(exception, exception.GetType(), 0, new Dictionary<object, IDictionary<string, object>>()))
            {
                data.Add(p.Key, p.Value);
            }
        }

        private object DestructureValue(object value, int level, IDictionary<object, IDictionary<string, object>> destructuredObjects)
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
                    .ToStringObjectDictionary()
                    .ToDictionary(e => e.Key, e => this.DestructureValue(e.Value, level + 1, destructuredObjects));
            }

            if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(valueTypeInfo))
            {
                if (destructuredObjects.ContainsKey(value))
                {
                    return new Dictionary<string, object>
                    {
                        { "$ref", "cyclic ref" }
                    };
                }

                destructuredObjects.Add(value, new Dictionary<string, object>());

                return ((IEnumerable)value)
                    .Cast<object>()
                    .Select(o => this.DestructureValue(o, level + 1, destructuredObjects))
                    .ToList();
            }

            return this.DestructureObject(value, valueType, level, destructuredObjects);
        }

        private IDictionary<string, object> DestructureObject(object value, Type valueType, int level, IDictionary<object, IDictionary<string, object>> destructuredObjects)
        {
            if (destructuredObjects.ContainsKey(value))
            {
                var id = destructuredObjects.Keys
                    .Select((v, i) => new { Value = v, Id = i + 1 })
                    .First(v => v.Value == value)
                    .Id;

                destructuredObjects[value]["$id"] = id.ToString();

                return new Dictionary<string, object>
                {
                    { "$ref", id.ToString() }
                };
            }

            var values = new Dictionary<string, object>();
            destructuredObjects.Add(value, values);

            var properties = valueType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanRead && x.GetIndexParameters().Length == 0);

            foreach (var property in properties)
            {
                try
                {
                    values.Add(property.Name, this.DestructureValue(property.GetValue(value), level + 1, destructuredObjects));
                }
                catch (TargetInvocationException targetInvocationException)
                {
                    var innerException = targetInvocationException.InnerException;
                    if (innerException != null)
                    {
                        values.Add(property.Name, $"threw {innerException.GetType().FullName}: {innerException.Message}");
                    }
                }
                catch (Exception exception)
                {
                    values.Add(property.Name, $"threw {exception.GetType().FullName}: {exception.Message}");
                }
            }

            values.Add("Type", valueType);

            return values;
        }
    }
}
