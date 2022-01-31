using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hyper
{
    internal static class TypeSupport
    {
        public static bool IsSupportedType<T>(bool allowNullable = true) => IsSupportedType(typeof(T), allowNullable);

        public static bool IsSupportedType(Type type, bool allowNullable = true)
        {
            switch (type)
            {
                case var t when t == typeof(float): return true;
                case var t when t == typeof(double): return true;
                case var t when t == typeof(decimal): return true;
                case var t when t == typeof(short): return true;
                case var t when t == typeof(int): return true;
                case var t when t == typeof(long): return true;
                case var t when t == typeof(ushort): return true;
                case var t when t == typeof(uint): return true;
                case var t when t == typeof(ulong): return true;
                case var t when t == typeof(sbyte): return true;
                case var t when t == typeof(byte): return true;
                case var t when t == typeof(char): return true;
                case var t when t == typeof(bool): return true;
                case var t when t == typeof(Guid): return true;
                case var t when t == typeof(TimeSpan): return true;
                case var t when t == typeof(DateTimeOffset): return true;
                case var t when t == typeof(DateTime): return true;
                case var t when t == typeof(string): return true;
                case var t when t.IsEnum: return true;
                case var t when t.IsPrimitive: return true;
                case var t when t.IsValueType && Nullable.GetUnderlyingType(t) == null: return true;
                case var t
                    when Nullable.GetUnderlyingType(t) != null && IsSupportedType(Nullable.GetUnderlyingType(t)):
                    return true;
                default: return false;
            };
        }
    }
    internal static class TypeSupportV3
    {
        public static bool IsSupportedType<T>(bool allowNullable = true) => IsSupportedType(typeof(T), allowNullable);

        public static bool IsSupportedType(Type type, bool allowNullable = true)
        {
            switch (type)
            {
                case var t when t == typeof(float): return true;
                case var t when t == typeof(double): return true;
                case var t when t == typeof(decimal): return true;
                case var t when t == typeof(short): return true;
                case var t when t == typeof(int): return true;
                case var t when t == typeof(long): return true;
                case var t when t == typeof(ushort): return true;
                case var t when t == typeof(uint): return true;
                case var t when t == typeof(ulong): return true;
                case var t when t == typeof(sbyte): return true;
                case var t when t == typeof(byte): return true;
                case var t when t == typeof(char): return true;
                case var t when t == typeof(bool): return true;
                case var t when t == typeof(Guid): return true;
                case var t when t == typeof(TimeSpan): return true;
                case var t when t == typeof(DateTimeOffset): return true;
                case var t when t == typeof(DateTime): return true;
                case var t when t == typeof(string): return true;
                case var t when t.IsArray && (t.GetElementType()?.IsValueType ?? false): return true;
                case var t when t == typeof(IEnumerable<>) && (t.GenericTypeArguments.FirstOrDefault()?.IsValueType ?? false): return true;
                case var t when t.IsEnum: return true;
                case var t when t.IsPrimitive: return true;
                case var t when t.IsValueType && Nullable.GetUnderlyingType(t) == null: return true;
                case var t
                    when Nullable.GetUnderlyingType(t) != null && IsSupportedType(Nullable.GetUnderlyingType(t)):
                    return true;
                default: return false;
            };
        }
    }
}