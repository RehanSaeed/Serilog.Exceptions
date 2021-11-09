namespace Serilog.Exceptions.Reflection;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Utility class that analyzes type using reflection and provides
/// information about properties to be used in destructuring process.
/// </summary>
internal class ReflectionInfoExtractor
{
    private readonly ConcurrentDictionary<Type, ReflectionInfo> reflectionInfoCache = new();
    private readonly IList<PropertyInfo> baseExceptionPropertiesForDestructuring;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReflectionInfoExtractor"/> class.
    /// </summary>
    public ReflectionInfoExtractor() => this.baseExceptionPropertiesForDestructuring = GetExceptionPropertiesForDestructuring(typeof(Exception));

    /// <summary>
    /// Gets reflection info for relevant properties of <paramref name="valueType"/>"/>
    /// from cache or by generating it if they are not yet present in cache.
    /// </summary>
    /// <param name="valueType">The type for which properties are to be analyzed.</param>
    /// <returns>The reflection info for relevant properties of <paramref name="valueType"/>.</returns>
    public ReflectionInfo GetOrCreateReflectionInfo(Type valueType) => this.reflectionInfoCache.GetOrAdd(valueType, valueFactory: this.GenerateReflectionInfoForType);

    private static Func<object, object> GenerateFastGetterForProperty(Type type, PropertyInfo property)
    {
        var objParam = Expression.Parameter(typeof(object), "num");
        var typedObj = Expression.Convert(objParam, type);
        var memberExpression = Expression.Property(typedObj, property);
        var typedResult = Expression.Convert(memberExpression, typeof(object));
        var resultLambda = Expression.Lambda<Func<object, object>>(typedResult, objParam);
        return resultLambda.Compile();
    }

    private static List<PropertyInfo> GetExceptionPropertiesForDestructuring(Type valueType)
    {
        var allProperties = valueType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.CanRead && x.GetIndexParameters().Length == 0)
            .ToList();

        return allProperties;
    }

    private static void MarkRedefinedPropertiesWithFullName(ReflectionPropertyInfo[] propertyInfos)
    {
        // First, prepare a dictionary of properties grouped by name
        var groupedByName = new Dictionary<string, List<ReflectionPropertyInfo>>();
        foreach (var propertyInfo in propertyInfos)
        {
            if (groupedByName.ContainsKey(propertyInfo.Name))
            {
                groupedByName[propertyInfo.Name].Add(propertyInfo);
            }
            else
            {
                groupedByName[propertyInfo.Name] = new List<ReflectionPropertyInfo> { propertyInfo };
            }
        }

        // Then, fix groups that have more than one property in it
        // It means that there is a name uniqueness conflict which needs to be resolved
        foreach (var nameAndProperties in groupedByName)
        {
            var properties = nameAndProperties.Value;
            if (properties.Count > 1)
            {
                FixGroupOfPropertiesWithConflictingNames(properties);
            }
        }
    }

    private static void FixGroupOfPropertiesWithConflictingNames(List<ReflectionPropertyInfo> properties)
    {
        // Very simplistic approach, just check each pair separately.
        // The implementation has O(N^2) complexity but in practice
        // N will be extremely rarely other than 2.
        foreach (var property in properties)
        {
            foreach (var otherProperty in properties)
            {
                property.MarkNameWithTypeNameIfRedefinesThatProperty(otherProperty);
            }
        }
    }

    private ReflectionInfo GenerateReflectionInfoForType(Type valueType)
    {
        var properties = GetExceptionPropertiesForDestructuring(valueType);
        var propertyInfos = properties
            .Select(p => new ReflectionPropertyInfo(p.Name, p.DeclaringType, GenerateFastGetterForProperty(valueType, p)))
            .ToArray();

        MarkRedefinedPropertiesWithFullName(propertyInfos);

        var propertiesInfosExceptBaseOnes = propertyInfos
            .Where(p => this.baseExceptionPropertiesForDestructuring.All(bp => bp.Name != p.Name))
            .ToArray();

        return new ReflectionInfo(propertyInfos, propertiesInfosExceptBaseOnes);
    }
}
