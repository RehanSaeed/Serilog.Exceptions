namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Serilog.Exceptions.Core;

    public class ReflectionBasedDestructurer : IExceptionDestructurer
    {
        private const string IdLabel = "$id";
        private const string RefLabel = "$ref";
        private const string CyclicReferenceMessage = "Cyclic reference";
        private const int MaxRecursiveLevel = 10;

        public Type[] TargetTypes => new[] { typeof(Exception) };

        public void Destructure(
            Exception exception,
            IExceptionPropertiesBag propertiesBag,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException)
        {
            var nextCyclicRefId = 1;
            foreach (var p in this.DestructureObject(
                exception,
                exception.GetType(),
                0,
                new Dictionary<object, IDictionary<string, object>>(),
                ref nextCyclicRefId))
            {
                propertiesBag.AddProperty(p.Key, p.Value);
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

            var destructuredDictionary = ((IDictionary)value).ToStringObjectDictionary();
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
                    values.Add(property.Name, this.DestructureValue(
                        property.GetValue(value),
                        level + 1,
                        destructuredObjects,
                        ref nextCyclicRefId));
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

            this.AppendTypeIfPossible(values, valueType);

            return values;
        }

        private void AppendTypeIfPossible(Dictionary<string, object> values, Type valueType)
        {
            if (values.ContainsKey("Type"))
            {
                if (!values.ContainsKey("$Type"))
                {
                    values.Add("$Type", valueType);
                }
                else
                {
                    // If both "Type" and "$Type" are present
                    // we just give up appending exception type
                }
            }
            else
            {
                values.Add("Type", valueType);
            }
        }
    }
}
