namespace Serilog.Exceptions.Reflection
{
    internal class ReflectionInfo
    {
        public ReflectionInfo(ReflectionPropertyInfo[] properties, ReflectionPropertyInfo[] propertiesExceptBaseOnes)
        {
            this.Properties = properties;
            this.PropertiesExceptBaseOnes = propertiesExceptBaseOnes;
        }

        public ReflectionPropertyInfo[] Properties { get; }

        public ReflectionPropertyInfo[] PropertiesExceptBaseOnes { get; }
    }
}
