namespace Serilog.Exceptions.Reflection
{
    using System;
    using System.Reflection;

    internal class ReflectionPropertyInfo
    {
        public ReflectionPropertyInfo(string name, Type? declaringType, Func<object, object> getter)
        {
            this.Name = name;
            this.DeclaringType = declaringType;
            this.Getter = getter;
        }

        public string Name { get; private set; }

        public Type? DeclaringType { get; }

        public Func<object, object> Getter { get; }

        private bool markedWithFullName = false;

        public void MarkNameWithFullName()
        {
            if (!this.markedWithFullName)
            {
                this.markedWithFullName = true;
                this.Name = $"{this.DeclaringType?.FullName}.{this.Name}";
            }
        }

        public void MarkNameWithFullNameIRedefineThatProperty(ReflectionPropertyInfo otherPropertyInfo)
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

            if (this.markedWithFullName)
            {
                return;
            }

            var shouldMarkWithFullName = IsSubTypeOf(
                otherPropertyInfo.DeclaringType,
                this.DeclaringType);

            if (shouldMarkWithFullName)
            {
                this.MarkNameWithFullName();
            }
        }

        private static bool IsSubTypeOf(Type possibleSubType, Type possibleBaseType) =>
#if NETSTANDARD1_3 || NETSTANDARD1_6
            possibleBaseType.GetTypeInfo().IsSubclassOf(possibleBaseType);
#else
            possibleSubType.IsSubclassOf(possibleBaseType);
#endif
    }
}
