using System;
using System.Reflection;

namespace Dapper
{
    internal static class TypeExtensions
    {
        public static bool IsValueType(this Type type)
        {
            return type.IsValueType;
        }
        public static bool IsEnum(this Type type)
        {
            return type.IsEnum;
        }
        public static TypeCode GetTypeCode(Type type)
        {
            return Type.GetTypeCode(type);
        }
        public static MethodInfo GetPublicInstanceMethod(this Type type, string name, Type[] types)
        {
            return type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public, null, types, null);
        }
    }
}