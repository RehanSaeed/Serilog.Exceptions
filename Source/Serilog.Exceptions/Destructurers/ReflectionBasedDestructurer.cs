namespace Serilog.Exceptions.Destructurers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Serilog.Exceptions.Core;

    /// <summary>
    /// Destructures exceptions by gathering all public non-indexer properties
    /// using reflection and then dynamically retrieving their values.
    /// This class can handle every exception inluding those with circular
    /// references and throwing properties. Additionally, a "Type" property
    /// is added to let the user know exact type of destructured exception.
    /// </summary>
    public class ReflectionBasedDestructurer : IExceptionDestructurer
    {
        private const string IdLabel = "$id";
        private const string RefLabel = "$ref";
        private const string CyclicReferenceMessage = "Cyclic reference";
        private readonly int destructuringDepth;
        private readonly object lockObj = new object();

        private readonly Dictionary<Type, ReflectionInfo> reflectionInfoCache = new Dictionary<Type, ReflectionInfo>();
        private readonly PropertyInfo[] baseExceptionPropertiesForDestructuring;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionBasedDestructurer"/> class.
        /// </summary>
        /// <param name="destructuringDepth">Maximum depth to which destructurer will go when destructuring exception object graph</param>
        public ReflectionBasedDestructurer(int destructuringDepth)
        {
            if (destructuringDepth <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(destructuringDepth),
                    destructuringDepth,
                    "Destructuring depth must be positive");
            }

            this.destructuringDepth = destructuringDepth;

            this.baseExceptionPropertiesForDestructuring = GetExceptionPropertiesForDestructuring(typeof(Exception));
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
            var destructuredObjects = new Dictionary<object, IDictionary<string, object>>();

            ExceptionDestructurer.DestructureCommonExceptionProperties(
                exception,
                propertiesBag,
                destructureException,
                data => this.DestructureValueDictionary(data, 1, destructuredObjects, destructureException, ref nextCyclicRefId));

            var reflectionInfo = this.GetOrCreateReflectionInfo(exception.GetType());

            this.AppendProperties(
                exception,
                reflectionInfo.PropertiesExceptBaseOnes,
                propertiesBag.AddProperty,
                destructuredObjects,
                level: 0,
                destructureException: destructureException,
                nextCyclicRefId: ref nextCyclicRefId);

            this.AppendTypeIfPossible(propertiesBag, exception.GetType());
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
            ParameterExpression objParam = Expression.Parameter(typeof(object), "num");
            UnaryExpression typedObj = Expression.Convert(objParam, type);
            MemberExpression memberExpression = Expression.Property(typedObj, property);
            UnaryExpression typedResult = Expression.Convert(memberExpression, typeof(object));
            Expression<Func<object, object>> resultLambda =
                Expression.Lambda<Func<object, object>>(
                    typedResult, objParam);
            return resultLambda.Compile();
        }

        private static PropertyInfo[] GetExceptionPropertiesForDestructuring(Type valueType)
        {
            return valueType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanRead && x.GetIndexParameters().Length == 0)
                .ToArray();
        }

        private void AppendProperties(
            object value,
            ReflectionPropertyInfo[] reflectionPropertyInfos,
            Action<string, object> addPropertyAction,
            IDictionary<object, IDictionary<string, object>> destructuredObjects,
            int level,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException,
            ref int nextCyclicRefId)
        {
            foreach (var property in reflectionPropertyInfos)
            {
                try
                {
                    object valueToBeDestructured = property.Getter(value);
                    int localNextCyclicRefId = nextCyclicRefId;
                    object destructuredValue = this.DestructureValue(
                        valueToBeDestructured,
                        level + 1,
                        destructuredObjects,
                        destructureException, ref localNextCyclicRefId);
                    nextCyclicRefId = localNextCyclicRefId;
                    addPropertyAction(property.Name, destructuredValue);
                }
                catch (TargetInvocationException targetInvocationException)
                {
                    var innerException = targetInvocationException.InnerException;
                    if (innerException != null)
                    {
                        addPropertyAction(property.Name, $"threw {innerException.GetType().FullName}: {innerException.Message}");
                    }
                }
                catch (Exception exception)
                {
                    addPropertyAction(property.Name, $"threw {exception.GetType().FullName}: {exception.Message}");
                }
            }
        }

        private ReflectionInfo GenerateReflectionInfoForType(Type valueType)
        {
            var properties = GetExceptionPropertiesForDestructuring(valueType);
            var propertyInfos = properties.Select(p => new ReflectionPropertyInfo()
            {
                Name = p.Name,
                Getter = GenerateFastGetterForProperty(valueType, p),
            }).ToArray();
            var propertiesInfosExceptBaseOnes = propertyInfos
                .Where(p => this.baseExceptionPropertiesForDestructuring.All(bp => bp.Name != p.Name))
                .ToArray();

            var reflectionInfo = new ReflectionInfo()
            {
                Properties = propertyInfos,
                PropertiesExceptBaseOnes = propertiesInfosExceptBaseOnes
            };
            return reflectionInfo;
        }

        private object DestructureValue(
            object value, int level, IDictionary<object, IDictionary<string, object>> destructuredObjects,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException,
            ref int nextCyclicRefId)
        {
            if (value == null)
            {
                return null;
            }

            var valueType = value.GetType();
            var valueTypeInfo = valueType.GetTypeInfo();

            if (value is MemberInfo)
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

            if (value is IDictionary dictionary)
            {
                return this.DestructureValueDictionary(dictionary, level, destructuredObjects, destructureException, ref nextCyclicRefId);
            }

            if (value is IEnumerable enumerable)
            {
                return this.DestructureValueEnumerable(enumerable, level, destructuredObjects, destructureException, ref nextCyclicRefId);
            }

            if (value is Uri uri)
            {
                return this.DestructureUri(uri);
            }

            if (value is CancellationToken ct)
            {
                return OperationCanceledExceptionDestructurer.DestructureCancellationToken(ct);
            }

            if (value is Task task)
            {
                return this.DestructureTask(task, destructuredObjects, destructureException, ref nextCyclicRefId);
            }

            return this.DestructureObject(value, valueType, level, destructuredObjects, destructureException, ref nextCyclicRefId);
        }

        private object DestructureValueEnumerable(
            IEnumerable value,
            int level,
            IDictionary<object, IDictionary<string, object>> destructuredObjects,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException,
            ref int nextCyclicRefId)
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
            foreach (var o in value.Cast<object>())
            {
                resultList.Add(this.DestructureValue(o, level + 1, destructuredObjects, destructureException, ref nextCyclicRefId));
            }

            return resultList;
        }

        private object DestructureUri(Uri value)
        {
            return value.ToString();
        }

        private object DestructureValueDictionary(
            IDictionary value, int level, IDictionary<object, IDictionary<string, object>> destructuredObjects,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException,
            ref int nextCyclicRefId)
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

            var destructuredDictionary = value.ToStringObjectDictionary();
            destructuredObjects.Add(value, destructuredDictionary);

            foreach (var kvp in destructuredDictionary.ToDictionary(k => k.Key, v => v.Value))
            {
                destructuredDictionary[kvp.Key] =
                    this.DestructureValue(kvp.Value, level + 1, destructuredObjects, destructureException, ref nextCyclicRefId);
            }

            return destructuredDictionary;
        }

        private IDictionary<string, object> DestructureObject(
            object value,
            Type valueType,
            int level,
            IDictionary<object, IDictionary<string, object>> destructuredObjects,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException,
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

            var reflectionInfo = this.GetOrCreateReflectionInfo(valueType);

            foreach (var property in reflectionInfo.Properties)
            {
                try
                {
                    object valueToBeDestructured = property.Getter(value);
                    object destructuredValue = this.DestructureValue(
                        valueToBeDestructured,
                        level + 1,
                        destructuredObjects,
                        destructureException, ref nextCyclicRefId);
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

            return values;
        }

        private object DestructureTask(
            Task task,
            IDictionary<object, IDictionary<string, object>> destructuredObjects,
            Func<Exception, IReadOnlyDictionary<string, object>> destructureException,
            ref int nextCyclicRefId)
        {
            if (destructuredObjects.TryGetValue(task, out var destructuredTask))
            {
                var refId = GetOrGenerateRefId(ref nextCyclicRefId, destructuredTask);

                return new SortedList<string, object>()
                {
                    { RefLabel, refId }
                };
            }

            var values = new SortedList<string, object>();
            destructuredObjects.Add(task, values);

            values[nameof(Task.Id)] = task.Id;
            values[nameof(Task.Status)] = task.Status.ToString("G");
            values[nameof(Task.CreationOptions)] = task.CreationOptions.ToString("F");
            if (task.IsFaulted)
            {
                values[nameof(Task.Exception)] = destructureException(task.Exception);
            }

            return values;
        }

        private ReflectionInfo GetOrCreateReflectionInfo(Type valueType)
        {
            lock (this.lockObj)
            {
                if (!this.reflectionInfoCache.TryGetValue(valueType, out var reflectionInfo))
                {
                    reflectionInfo = this.GenerateReflectionInfoForType(valueType);
                    this.reflectionInfoCache.Add(valueType, reflectionInfo);
                }

                return reflectionInfo;
            }
        }

        private void AppendTypeIfPossible(IExceptionPropertiesBag propertiesBag, Type valueType)
        {
            if (propertiesBag.ContainsProperty("Type"))
            {
                if (!propertiesBag.ContainsProperty("$Type"))
                {
                    propertiesBag.AddProperty("$Type", valueType.FullName);
                }
                else
                {
                    // If both "Type" and "$Type" are present
                    // we just give up appending exception type
                }
            }
            else
            {
                propertiesBag.AddProperty("Type", valueType.FullName);
            }
        }

        private class ReflectionInfo
        {
            public ReflectionPropertyInfo[] Properties { get; set; }

            public ReflectionPropertyInfo[] PropertiesExceptBaseOnes { get; set; }
        }

        private class ReflectionPropertyInfo
        {
            public string Name { get; set; }

            public Func<object, object> Getter { get; set; }
        }
    }
}
