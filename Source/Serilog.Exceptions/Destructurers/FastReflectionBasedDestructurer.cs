namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using Serilog.Exceptions.Core;

    /// <summary>
    /// Destructures exceptions by gathering all public non-indexer properties
    /// using reflection and then dynamically retrieving their values.
    /// This class can handle every exception inluding those with circular
    /// references and throwing properties. Additionally, a "Type" property
    /// is added to let the user know exact type of destructured exception.
    /// </summary>
    public class FastReflectionBasedDestructurer : IExceptionDestructurer
    {
        private const string IdLabel = "$id";
        private const string RefLabel = "$ref";
        private const string CyclicReferenceMessage = "Cyclic reference";
        private readonly int destructuringDepth;

        private readonly Dictionary<Type, ReflectionInfo> reflectionInfoCache = new Dictionary<Type, ReflectionInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FastReflectionBasedDestructurer"/> class.
        /// </summary>
        /// <param name="destructuringDepth">Maximum depth to which destructurer will go when destructuring exception object graph</param>
        public FastReflectionBasedDestructurer(int destructuringDepth)
        {
            if (destructuringDepth <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(destructuringDepth),
                    destructuringDepth,
                    "Destructuring depth must be positive");
            }

            this.destructuringDepth = destructuringDepth;
        }

        /// <inheritdoc cref="IExceptionDestructurer.TargetTypes"/>
        public Type[] TargetTypes => new[] { typeof(Exception) };

        /// <inheritdoc cref="IExceptionDestructurer.Destructure"/>
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

        private static Func<object, object> GenerateFastGetterForProperty(Type type, PropertyInfo property)
        {
#if NETSTANDARD1_3 || NETSTANDARD1_6
            return (x) => property.GetValue(x);
#else
            DynamicMethod dynamicMethod = new DynamicMethod(
                "ExtractProperty",
                typeof(object),
                new[] { typeof(object) });

            var getMethod = property.GetGetMethod();

            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Castclass, type);
            ilGenerator.EmitCall(OpCodes.Callvirt, getMethod, null);
            if (IsValueType(property.PropertyType))
            {
                ilGenerator.Emit(OpCodes.Box, property.PropertyType);
            }

            ilGenerator.Emit(OpCodes.Ret);

            return (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
#endif
        }

        private static bool IsValueType(Type type)
        {
#if NETSTANDARD1_3 || NETSTANDARD1_6
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }

        private static ReflectionInfo GenerateReflectionInfoForType(Type valueType)
        {
            var properties = valueType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanRead && x.GetIndexParameters().Length == 0)
                .ToArray();

            var reflectionInfo = new ReflectionInfo()
            {
                Properties = properties.Select(p => new ReflectionPropertyInfo()
                {
                    Name = p.Name,
                    Getter = GenerateFastGetterForProperty(valueType, p)
                }).ToArray()
            };
            return reflectionInfo;
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

            if (level > this.destructuringDepth)
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

            if (typeof(Uri).GetTypeInfo().IsAssignableFrom(valueTypeInfo))
            {
                return this.DestructureUri((Uri)value);
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

        private object DestructureUri(Uri value)
        {
            return value.ToString();
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

            if (!this.reflectionInfoCache.TryGetValue(valueType, out var reflectionInfo))
            {
                reflectionInfo = GenerateReflectionInfoForType(valueType);
                this.reflectionInfoCache.Add(valueType, reflectionInfo);
            }

            foreach (var property in reflectionInfo.Properties)
            {
                try
                {
                    object valueToBeDestructured = property.Getter(value);
                    object destructuredValue = this.DestructureValue(
                        valueToBeDestructured,
                        level + 1,
                        destructuredObjects,
                        ref nextCyclicRefId);
                    values.Add(property.Name, destructuredValue);
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

        private class ReflectionInfo
        {
            public ReflectionPropertyInfo[] Properties { get; set; }
        }

        private class ReflectionPropertyInfo
        {
            public string Name { get; set; }

            public Func<object, object> Getter { get; set; }
        }
    }
}
