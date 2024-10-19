namespace Serilog.Exceptions.Destructurers;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Reflection;

/// <summary>
/// Destructures exceptions by gathering all public non-indexer properties using reflection and then dynamically
/// retrieving their values. This class can handle every exception including those with circular references and
/// throwing properties. Additionally, a "Type" property is added to let the user know exact type of destructured
/// exception.
/// </summary>
public class ReflectionBasedDestructurer : IExceptionDestructurer
{
    private const string IdLabel = "$id";
    private const string RefLabel = "$ref";
    private const string CyclicReferenceMessage = "Cyclic reference";
    private const string IQueryableDestructureSkippedMessage = "IQueryable skipped";
    private readonly int destructuringDepth;
    private readonly ReflectionInfoExtractor reflectionInfoExtractor = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReflectionBasedDestructurer"/> class.
    /// </summary>
    /// <param name="destructuringDepth">Maximum depth to which the destructurer will go when destructuring an
    /// exception object graph.</param>
    public ReflectionBasedDestructurer(int destructuringDepth)
    {
        if (destructuringDepth <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(destructuringDepth),
                destructuringDepth,
                Resources.DestructuringDepthMustBeMoreThanZero);
        }

        this.destructuringDepth = destructuringDepth;
    }

    /// <inheritdoc cref="IExceptionDestructurer.TargetTypes"/>
#pragma warning disable CA1819 // Properties should not return arrays
    public Type[] TargetTypes => [typeof(Exception)];
#pragma warning restore CA1819 // Properties should not return arrays

    /// <inheritdoc cref="IExceptionDestructurer.Destructure"/>
    public void Destructure(
        Exception exception,
        IExceptionPropertiesBag propertiesBag,
        Func<Exception, IReadOnlyDictionary<string, object?>?> destructureException)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(propertiesBag);
        ArgumentNullException.ThrowIfNull(destructureException);
#else
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        if (propertiesBag is null)
        {
            throw new ArgumentNullException(nameof(propertiesBag));
        }

        if (destructureException is null)
        {
            throw new ArgumentNullException(nameof(destructureException));
        }
#endif

        var nextCyclicRefId = 1;
        var destructuredObjects = new Dictionary<object, IDictionary<string, object?>>();

        ExceptionDestructurer.DestructureCommonExceptionProperties(
            exception,
            propertiesBag,
            destructureException,
            data => this.DestructureValueDictionary(data, 1, destructuredObjects, ref nextCyclicRefId));

        var reflectionInfo = this.reflectionInfoExtractor.GetOrCreateReflectionInfo(exception.GetType());

        this.AppendProperties(
            exception,
            reflectionInfo.PropertiesExceptBaseOnes,
            propertiesBag.AddProperty,
            destructuredObjects,
            level: 0,
            nextCyclicRefId: ref nextCyclicRefId);

        AppendTypeIfPossible(propertiesBag, exception.GetType());
    }

    private static string? GetOrGenerateRefId(ref int nextCyclicRefId, IDictionary<string, object?> destructuredObject)
    {
        string? refId;
        if (destructuredObject.TryGetValue(IdLabel, out var value))
        {
            refId = (string?)value;
        }
        else
        {
            var id = nextCyclicRefId;
            nextCyclicRefId++;
            refId = id.ToString(CultureInfo.InvariantCulture);
            destructuredObject[IdLabel] = refId;
        }

        return refId;
    }

    private static object DestructureUri(Uri value) => value.ToString();

    private static void AppendTypeIfPossible(IExceptionPropertiesBag propertiesBag, Type valueType)
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

    private void AppendProperties(
        object value,
        ReflectionPropertyInfo[] reflectionPropertyInfos,
        Action<string, object?> addPropertyAction,
        IDictionary<object, IDictionary<string, object?>> destructuredObjects,
        int level,
        ref int nextCyclicRefId)
    {
        foreach (var property in reflectionPropertyInfos)
        {
            try
            {
                var valueToBeDestructured = property.Getter(value);
                var localNextCyclicRefId = nextCyclicRefId;
                var destructuredValue = this.DestructureValue(
                    valueToBeDestructured,
                    level + 1,
                    destructuredObjects,
                    ref localNextCyclicRefId);
                nextCyclicRefId = localNextCyclicRefId;
                addPropertyAction(property.Name, destructuredValue);
            }
            catch (TargetInvocationException targetInvocationException)
            {
                var innerException = targetInvocationException.InnerException;
                if (innerException is not null)
                {
                    addPropertyAction(property.Name, $"threw {innerException.GetType().FullName}: {innerException.Message}");
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                addPropertyAction(property.Name, $"threw {exception.GetType().FullName}: {exception.Message}");
            }
        }
    }

    private object? DestructureValue(
        object? value,
        int level,
        IDictionary<object, IDictionary<string, object?>> destructuredObjects,
        ref int nextCyclicRefId)
    {
        if (value is null)
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
            return this.DestructureValueDictionary(dictionary, level, destructuredObjects, ref nextCyclicRefId);
        }

        if (value is IQueryable)
        {
            return IQueryableDestructureSkippedMessage;
        }
        else if (value is IEnumerable enumerable)
        {
            return this.DestructureValueEnumerable(enumerable, level, destructuredObjects, ref nextCyclicRefId);
        }

        if (value is Uri uri)
        {
            return DestructureUri(uri);
        }

        if (value is CancellationToken ct)
        {
            return OperationCanceledExceptionDestructurer.DestructureCancellationToken(ct);
        }

        if (value is Task task)
        {
            return this.DestructureTask(task, level, destructuredObjects, ref nextCyclicRefId);
        }

        return this.DestructureObject(value, valueType, level, destructuredObjects, ref nextCyclicRefId);
    }

    private object DestructureValueEnumerable(
        IEnumerable value,
        int level,
        IDictionary<object, IDictionary<string, object?>> destructuredObjects,
        ref int nextCyclicRefId)
    {
        if (destructuredObjects.ContainsKey(value))
        {
            return new Dictionary<string, object>
                {
                    { RefLabel, CyclicReferenceMessage },
                };
        }

        destructuredObjects.Add(value, new Dictionary<string, object?>());

        var resultList = new List<object?>();
        foreach (var o in value.Cast<object>())
        {
            resultList.Add(this.DestructureValue(o, level + 1, destructuredObjects, ref nextCyclicRefId));
        }

        return resultList;
    }

    private object DestructureValueDictionary(
        IDictionary value,
        int level,
        IDictionary<object, IDictionary<string, object?>> destructuredObjects,
        ref int nextCyclicRefId)
    {
        if (destructuredObjects.TryGetValue(value, out var destructuredObject))
        {
            var refId = GetOrGenerateRefId(ref nextCyclicRefId, destructuredObject);

            return new Dictionary<string, object?>
                {
                    { RefLabel, refId },
                };
        }

        var destructuredDictionary = value.ToStringObjectDictionary();
        destructuredObjects.Add(value, destructuredDictionary);

        foreach (var kvp in destructuredDictionary.ToDictionary(k => k.Key, v => v.Value))
        {
            destructuredDictionary[kvp.Key] =
                this.DestructureValue(kvp.Value, level + 1, destructuredObjects, ref nextCyclicRefId);
        }

        return destructuredDictionary;
    }

    private IDictionary<string, object?> DestructureObject(
        object value,
        Type valueType,
        int level,
        IDictionary<object, IDictionary<string, object?>> destructuredObjects,
        ref int nextCyclicRefId)
    {
        if (destructuredObjects.TryGetValue(value, out var destructuredObject))
        {
            var refId = GetOrGenerateRefId(ref nextCyclicRefId, destructuredObject);

            return new Dictionary<string, object?>()
                {
                    { RefLabel, refId },
                };
        }

        var values = new Dictionary<string, object?>();
        destructuredObjects.Add(value, values);

        var reflectionInfo = this.reflectionInfoExtractor.GetOrCreateReflectionInfo(valueType);

        foreach (var property in reflectionInfo.Properties)
        {
            try
            {
                var valueToBeDestructured = property.Getter(value);
                var destructuredValue = this.DestructureValue(
                    valueToBeDestructured,
                    level + 1,
                    destructuredObjects,
                    ref nextCyclicRefId);
                values.Add(property.Name, destructuredValue);
            }
            catch (TargetInvocationException targetInvocationException)
            {
                var innerException = targetInvocationException.InnerException;
                if (innerException is not null)
                {
                    values.Add(property.Name, $"threw {innerException.GetType().FullName}: {innerException.Message}");
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                values.Add(property.Name, $"threw {exception.GetType().FullName}: {exception.Message}");
            }
        }

        return values;
    }

    private object DestructureTask(
        Task task,
        int level,
        IDictionary<object, IDictionary<string, object?>> destructuredObjects,
        ref int nextCyclicRefId)
    {
        if (destructuredObjects.TryGetValue(task, out var destructuredTask))
        {
            var refId = GetOrGenerateRefId(ref nextCyclicRefId, destructuredTask);

            return new SortedList<string, object?>()
                {
                    { RefLabel, refId },
                };
        }

        var values = new SortedList<string, object?>();
        destructuredObjects.Add(task, values);

        values[nameof(Task.Id)] = task.Id;
        values[nameof(Task.Status)] = task.Status.ToString("G");
        values[nameof(Task.CreationOptions)] = task.CreationOptions.ToString("F");
        if (task.IsFaulted)
        {
            values[nameof(Task.Exception)] = this.DestructureValue(task.Exception, level, destructuredObjects, ref nextCyclicRefId);
        }

        return values;
    }
}
