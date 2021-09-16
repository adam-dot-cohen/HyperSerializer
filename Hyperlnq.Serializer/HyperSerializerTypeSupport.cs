using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hyperlnq.Serializer
{
    public static class HyperSerializerTypeSupport
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


    //public static class SerializerMemberInfoFactory
    //{
    //    public static bool IsSupportedType(MemberInfo type, out SerializeMemberInfo info, bool isNullable = false)
    //    {
    //        info = default;
    //        var memberType = type.GetType();
    //        Type? nullable;
    //        if((nullable).GetUnderlyingType(memberType))
    //        if(primatives.Contains(memberType) || memberType.IsEnum || memberType.IsValueType || memberType)



    //        info = new SerializeMemberInfo<T>();
    //        info.
    //        typeof(string):
    //        case var t when t.IsEnum: return true;
    //        case var t when t.IsPrimitive: return true;
    //        case var t when t.IsValueType && Nullable.GetUnderlyingType(t) == null: return true;
    //        case var t when Nullable.GetUnderlyingType(t) != null &&
    //            IsSupportedType<T>(Nullable.GetUnderlyingType(t), out info, true):
    //        info.IsBittableType()
    //        return true;
    //        default: return false;
    //    }


    //    private static bool IsSupported(Type type)
    //    {
    //        return type switch
    //        {
    //            TypeInfo typeInfo => throw new NotImplementedException(),
    //            var tf when tf == typeof(float) => true,
    //            _ => throw new ArgumentOutOfRangeException(nameof(type)),
    //        };
    //    }
        
    //    static HashSet<Type> primatives = new HashSet<Type>()
    //    {
    //        typeof(float),
    //        typeof(double),
    //        typeof(decimal),
    //        typeof(short),
    //        typeof(int),
    //        typeof(long),
    //        typeof(ushort),
    //        typeof(uint),
    //        typeof(ulong),
    //        typeof(sbyte),
    //        typeof(byte),
    //        typeof(char),
    //        typeof(bool),
    //        typeof(Guid),
    //        typeof(TimeSpan),
    //        typeof(DateTimeOffset),
    //        typeof(DateTime)
    //    };
    //}

    //public class SerializeMemberInfo
    //{
    //    public SerializeMemberInfo(MemberInfo info) => MemberInfo = info;
    //    public MemberInfo MemberInfo { get; init; }
    //    public Type MemberType => MemberInfo.get;
    //    public Type? UnderlyingType => Nullable.GetUnderlyingType(MemberType);
    //    public bool IsNullable => UnderlyingType != null;
    //    public string ParameterName
    //    {
    //        get
    //        {
    //            switch (typeof(T).MemberType)
    //            {
    //                case MemberTypes.Event:
    //                    return ((EventInfo)member).EventHandlerType;
    //                case MemberTypes.Field:
    //                    return ((FieldInfo)member).FieldType;
    //                case MemberTypes.Method:
    //                    return ((MethodInfo)member).ReturnType;
    //                case MemberTypes.Property:
    //                    return ((PropertyInfo)member).PropertyType;
    //                default:
    //                    throw new ArgumentException
    //                    (
    //                        "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
    //                    );
    //            }
    //        }
    //    }
    //    public string TypeName
    //    public int? SizeOf => typeof(T).SizeOf();
    //}
}