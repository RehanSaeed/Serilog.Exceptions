namespace Serilog.Exceptions
{
    using System;
    using System.Reflection;

    internal static class TypeExtensions
    {
        public static TypeCode GetTypeCode(this Type type)
        {
#if NETSTANDARD1_3
            if (type == null)
            {
                return TypeCode.Empty;
            }

            if (type == (typeof(bool)))
            {
                return TypeCode.Boolean;
            }
            else if (type == (typeof(char)))
            {
                return TypeCode.Char;
            }
            else if (type == (typeof(sbyte)))
            {
                return TypeCode.SByte;
            }
            else if (type == (typeof(byte)))
            {
                return TypeCode.Byte;
            }
            else if (type == (typeof(short)))
            {
                return TypeCode.Int16;
            }
            else if (type == (typeof(ushort)))
            {
                return TypeCode.UInt16;
            }
            else if (type == (typeof(int)))
            {
                return TypeCode.Int32;
            }
            else if (type == (typeof(uint)))
            {
                return TypeCode.UInt32;
            }
            else if (type == (typeof(long)))
            {
                return TypeCode.Int64;
            }
            else if (type == (typeof(ulong)))
            {
                return TypeCode.UInt64;
            }
            else if (type == (typeof(float)))
            {
                return TypeCode.Single;
            }
            else if (type == (typeof(double)))
            {
                return TypeCode.Double;
            }
            else if (type == (typeof(decimal)))
            {
                return TypeCode.Decimal;
            }
            else if (type == (typeof(DateTime)))
            {
                return TypeCode.DateTime;
            }
            else if (type == (typeof(string)))
            {
                return TypeCode.String;
            }
            else if (type.GetTypeInfo().IsEnum)
            {
                return TypeCode.Int32;
            }

            return TypeCode.Object;
#else
            return Type.GetTypeCode(type);
#endif
        }
    }
}