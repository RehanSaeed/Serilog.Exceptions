namespace Serilog.Exceptions.Reflection
{
    using System;

    internal class ReflectionPropertyInfo
    {
        public ReflectionPropertyInfo(string name, Type? declaringType, Func<object, object> getter)
        {
            this.Name = name;
            this.DeclaringType = declaringType;
            this.Getter = getter;
        }

        public string Name { get; }

        public Type? DeclaringType { get; }

        public Func<object, object> Getter { get; }
    }
}
