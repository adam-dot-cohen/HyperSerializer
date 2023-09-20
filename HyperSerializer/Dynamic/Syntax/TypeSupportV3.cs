using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HyperSerializer.Dynamic.Syntax;

internal static class TypeSupport
{
    public static bool IsSupportedType<T>() => IsSupportedType(typeof(T));

    public static bool IsSupportedType(Type type)
    {
        switch (type)
        {
            case var t when t == typeof(string): return true;
            case var t when t.IsValueType: return true;
            case var t when t.IsValueType && Nullable.GetUnderlyingType(t) == null: return true;
            case var t when t.IsAssignableFrom(typeof(IEnumerable)): return true;
            case var t when t.IsArray && (t.GetElementType()?.IsValueType ?? false): return true;
            case var t when t == typeof(IEnumerable<>) && (t.GenericTypeArguments.FirstOrDefault()?.IsValueType ?? false): return true;
            case var t when t.IsEnum: return true;
            case var t when t.IsPrimitive: return true;
            case var t
                when Nullable.GetUnderlyingType(t) != null && IsSupportedType(Nullable.GetUnderlyingType(t)):
                return true;
            default:
                return false;
        };
    }
}