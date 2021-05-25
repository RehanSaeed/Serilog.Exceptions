namespace Serilog.Exceptions.Reflection
{
    using System;
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
        private readonly object lockObj = new();
        private readonly Dictionary<Type, ReflectionInfo> reflectionInfoCache = new();
        private readonly IList<PropertyInfo> baseExceptionPropertiesForDestructuring;

        public ReflectionInfoExtractor() => this.baseExceptionPropertiesForDestructuring = GetExceptionPropertiesForDestructuring(typeof(Exception));

        public ReflectionInfo GetOrCreateReflectionInfo(Type valueType)
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
            for (var i = 0; i < properties.Count; i++)
            {
                for (var j = i + 1; j < properties.Count; j++)
                {
                    var property1 = properties[i];
                    var property2 = properties[j];
                    property2.MarkNameWithFullNameIfRedefinesThatProperty(property1);
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
}
