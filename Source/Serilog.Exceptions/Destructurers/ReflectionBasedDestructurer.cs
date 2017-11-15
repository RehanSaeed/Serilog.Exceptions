namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class ReflectionBasedDestructurer : IExceptionDestructurer
    {
        private const string IdLabel = "$id";
        private const string RefLabel = "$ref";
        private const string CyclicReferenceMessage = "Cyclic reference";
        private const int MaxRecursiveLevel = 10;

        public ReflectionBasedDestructurer(List<string> ignoredProperties)
        {
            this.IgnoredProperties = ignoredProperties;
        }

        public List<string> IgnoredProperties { get; set; }

        public Type[] TargetTypes => new Type[] { typeof(Exception) };

        public void Destructure(
            Exception exception,
            IDictionary<string, object> data,
            Func<Exception, IDictionary<string, object>> destructureException)
        {
            var nextCyclicRefId = 1;
            foreach (var p in this.DestructureObject(
                exception,
                exception.GetType(),
                0,
                new Dictionary<object, IDictionary<string, object>>(),
                ref nextCyclicRefId))
            {
                data.AddIfNotIgnored(p.Key, p.Value, this.IgnoredProperties);
            }
        }

        private static string GetOrGenerateRefId(ref int nextCyclicRefId, IDictionary<string, object> destructuredObject)
        {
            string refId;
            if (destructuredObject.ContainsKey(IdLabel))
            {
                refId = (string)destructuredObject[IdLabel];
            }
            else
            {
                var id = nextCyclicRefId;
                nextCyclicRefId++;
                refId = id.ToString();
                destructuredObject[IdLabel] = refId;
            }

            return refId;
        }

        private object DestructureValue(object value, int level, IDictionary<object, IDictionary<string, object>> destructuredObjects, ref int nextCyclicRefId)
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
                return this.DestructureValueDictionary(value, level, destructuredObjects, ref nextCyclicRefId);
            }

            if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(valueTypeInfo))
            {
                return this.DestructureValueEnumerable(value, level, destructuredObjects, ref nextCyclicRefId);
            }

            return this.DestructureObject(value, valueType, level, destructuredObjects, ref nextCyclicRefId);
        }

        private object DestructureValueEnumerable(object value, int level, IDictionary<object, IDictionary<string, object>> destructuredObjects, ref int nextCyclicRefId)
        {
            if (destructuredObjects.ContainsKey(value))
            {
                return new Dictionary<string, object>
                {
                    { RefLabel, CyclicReferenceMessage }
                };
            }

            destructuredObjects.Add(value, new Dictionary<string, object>());

            var resultList = new List<object>();
            foreach (var o in ((IEnumerable)value).Cast<object>())
            {
                resultList.Add(this.DestructureValue(o, level + 1, destructuredObjects, ref nextCyclicRefId));
            }

            return resultList;
        }

        private object DestructureValueDictionary(object value, int level, IDictionary<object, IDictionary<string, object>> destructuredObjects, ref int nextCyclicRefId)
        {
            if (destructuredObjects.ContainsKey(value))
            {
                IDictionary<string, object> destructuredObject = destructuredObjects[value];
                var refId = GetOrGenerateRefId(ref nextCyclicRefId, destructuredObject);

                return new Dictionary<string, object>
                {
                    { RefLabel, refId }
                };
            }

            var destructuredDictionary = ((IDictionary)value).ToStringObjectDictionary(this.IgnoredProperties);
            destructuredObjects.Add(value, destructuredDictionary);

            foreach (var kvp in destructuredDictionary.ToDictionary(k => k.Key, v => v.Value))
            {
                destructuredDictionary[kvp.Key] =
                        this.DestructureValue(kvp.Value, level + 1, destructuredObjects, ref nextCyclicRefId);
            }

            return destructuredDictionary;
        }

        private IDictionary<string, object> DestructureObject(
            object value,
            Type valueType,
            int level,
            IDictionary<object, IDictionary<string, object>> destructuredObjects,
            ref int nextCyclicRefId)
        {
            if (destructuredObjects.ContainsKey(value))
            {
                var destructuredObject = destructuredObjects[value];
                var refId = GetOrGenerateRefId(ref nextCyclicRefId, destructuredObject);

                return new Dictionary<string, object>()
                {
                    { RefLabel, refId }
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
                    values.AddIfNotIgnored(
                        property.Name,
                        this.DestructureValue(
                        property.GetValue(value),
                        level + 1,
                        destructuredObjects,
                        ref nextCyclicRefId),
                        this.IgnoredProperties);
                }
                catch (TargetInvocationException targetInvocationException)
                {
                    var innerException = targetInvocationException.InnerException;
                    if (innerException != null)
                    {
                        values.AddIfNotIgnored(property.Name, $"threw {innerException.GetType().FullName}: {innerException.Message}", this.IgnoredProperties);
                    }
                }
                catch (Exception exception)
                {
                    values.AddIfNotIgnored(property.Name, $"threw {exception.GetType().FullName}: {exception.Message}", this.IgnoredProperties);
                }
            }

            this.AppendTypeIfPossible(values, valueType);
            return values;
        }

        private void AppendTypeIfPossible(Dictionary<string, object> values, Type valueType)
        {
            if (values.ContainsKey("Type"))
            {
                if (!values.ContainsKey("$Type"))
                {
                    values.AddIfNotIgnored("$Type", valueType, this.IgnoredProperties);
                }
                else
                {
                    // If both "Type" and "$Type" are present
                    // we just give up appending exception type
                }
            }
            else
            {
                values.AddIfNotIgnored("Type", valueType, this.IgnoredProperties);
            }
        }
    }
}
