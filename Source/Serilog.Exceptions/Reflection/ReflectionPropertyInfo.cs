namespace Serilog.Exceptions.Reflection
{
    using System;
#if NETSTANDARD1_3
    using System.Reflection;
#endif

    /// <summary>
    /// Class that holds reflection information about a single property.
    /// </summary>
    internal class ReflectionPropertyInfo
    {
        /// <summary>
        /// Marker that represents a decision whether to extend property name
        /// with type name of declaring type, to avoid name uniqueness conflicts.
        /// </summary>
        private bool markedWithTypeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionPropertyInfo"/> class.
        /// </summary>
        /// <param name="name">The of the property without type name.</param>
        /// <param name="declaringType">The type which declares the property.</param>
        /// <param name="getter">Runtime function that can extract value of the property from object.</param>
        public ReflectionPropertyInfo(string name, Type? declaringType, Func<object, object> getter)
        {
            this.Name = name;
            this.DeclaringType = declaringType;
            this.Getter = getter;
        }

        /// <summary>
        /// Gets name of the property.
        /// It includes type name if name conflicts with other property of derived class.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the type that declared the property.
        /// </summary>
        public Type? DeclaringType { get; }

        /// <summary>
        /// Gets a function that extracts value of the property from an object.
        /// </summary>
        public Func<object, object> Getter { get; }

        /// <summary>
        /// Idempotent action that extends property name with a type name
        /// of its declaring type to avoid name uniqueness conflict.
        /// </summary>
        public void MarkNameWithTypeName()
        {
            if (!this.markedWithTypeName)
            {
                this.markedWithTypeName = true;
                this.Name = $"{this.DeclaringType?.Name}.{this.Name}";
            }
        }

        /// <summary>
        /// Conditionally marks property with its declaring type name to avoid
        /// name uniqueness conflict.
        /// </summary>
        /// <param name="otherPropertyInfo">Other property info that is be compared to detect name uniqueness conflict.</param>
        public void MarkNameWithTypeNameIfRedefinesThatProperty(ReflectionPropertyInfo otherPropertyInfo)
        {
            if (otherPropertyInfo == this)
            {
                return;
            }

            if (this.DeclaringType == null || otherPropertyInfo?.DeclaringType == null)
            {
                return;
            }

            if (otherPropertyInfo?.Name != this.Name)
            {
                return;
            }

            if (this.markedWithTypeName)
            {
                return;
            }

            var shouldMarkWithTypeName = IsSubTypeOf(
                otherPropertyInfo.DeclaringType,
                this.DeclaringType);

            if (shouldMarkWithTypeName)
            {
                this.MarkNameWithTypeName();
            }
        }

        private static bool IsSubTypeOf(Type possibleSubType, Type possibleBaseType) =>
#if NETSTANDARD1_3
            possibleBaseType.GetTypeInfo().IsSubclassOf(possibleBaseType);
#else
            possibleSubType.IsSubclassOf(possibleBaseType);
#endif
    }
}
