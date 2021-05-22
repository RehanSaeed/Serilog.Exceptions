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
        private readonly PropertyInfo[] baseExceptionPropertiesForDestructuring;

        public ReflectionInfoExtractor()
        {
            this.baseExceptionPropertiesForDestructuring = GetExceptionPropertiesForDestructuring(typeof(Exception));

        }


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

        private ReflectionInfo GenerateReflectionInfoForType(Type valueType)
        {
            var properties = GetExceptionPropertiesForDestructuring(valueType);
            var propertyInfos = properties
                .Select(p => new ReflectionPropertyInfo(p.Name, p.DeclaringType, GenerateFastGetterForProperty(valueType, p)))
                .ToArray();
            var propertiesInfosExceptBaseOnes = propertyInfos
                .Where(p => this.baseExceptionPropertiesForDestructuring.All(bp => bp.Name != p.Name))
                .ToArray();

            return new ReflectionInfo(propertyInfos, propertiesInfosExceptBaseOnes);
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

        private static PropertyInfo[] GetExceptionPropertiesForDestructuring(Type valueType) =>
            valueType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanRead && x.GetIndexParameters().Length == 0)
                .ToArray();
    }
}
