namespace Serilog.Exceptions.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

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

        private ReflectionInfo GenerateReflectionInfoForType(Type valueType)
        {
            var properties = GetExceptionPropertiesForDestructuring(valueType);
            var propertyInfos = properties
                .Select(p => new ReflectionPropertyInfo(p.Name, p.DeclaringType, GenerateFastGetterForProperty(valueType, p)))
                .ToArray();

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

            foreach (var nameGroup in groupedByName)
            {
                if (nameGroup.Value.Count > 1)
                {
                    foreach (var propertyInfoInGroupName in nameGroup.Value)
                    {
                        foreach (var otherPropertyInfoInGroupName in nameGroup.Value)
                        {
                            propertyInfoInGroupName.MarkNameWithFullNameIRedefineThatProperty(otherPropertyInfoInGroupName);
                        }
                    }
                }
            }

            var propertiesInfosExceptBaseOnes = propertyInfos
                .Where(p => this.baseExceptionPropertiesForDestructuring.All(bp => bp.Name != p.Name))
                .ToArray();

            return new ReflectionInfo(propertyInfos, propertiesInfosExceptBaseOnes);
        }



    }
}
