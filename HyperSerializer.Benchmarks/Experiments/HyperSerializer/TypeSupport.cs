using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HyperSerializer.CodeGen;

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
 internal static class ReflectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo As(this MethodInfo obj)
            => obj;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBittableType<T>(this T type)
        {
            bool isBittable = false;
            try
            {
                if (default(T) != null)
                {
                    // Non-blittable types cannot allocate pinned handle
                    GCHandle.Alloc(default(T), GCHandleType.Pinned).Free();
                    isBittable = true;
                }
            }
            catch { }
            return isBittable;
        }
        public static int SizeOf<TType>() => typeof(TType).SizeOf();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int sizeOF(this Type type)
        {
            if (_typeSizes.ContainsKey(type)) return _typeSizes[type];
            var method = new DynamicMethod("GetManagedSizeImpl", typeof(uint), new Type[0], typeof(TypeExtensions), false);
            ILGenerator gen = method.GetILGenerator();
            gen.Emit(OpCodes.Sizeof, type);
            gen.Emit(OpCodes.Ret);
            var size = checked((int)((Func<uint>)method.CreateDelegate(typeof(Func<uint>)))());
            lock (_typeSizes)
                _typeSizes.Add(type, size);
            return size;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetClassName<T>(this Type obj)
        {
            var cType = Nullable.GetUnderlyingType(typeof(T));
            string cTypeName = default;

            if (cType != null)
                cTypeName = $"{cType.Name}N";
            else
                cTypeName = typeof(T).Name;
            return cTypeName;
        }
        public static int SizeOf(this Type type)
        {
            switch (type)
            {
                case var t when t == typeof(float): return Unsafe.SizeOf<float>();
                case var t when t == typeof(double): return Unsafe.SizeOf<double>();
                case var t when t == typeof(decimal): return Unsafe.SizeOf<decimal>();
                case var t when t == typeof(short): return Unsafe.SizeOf<short>();
                case var t when t == typeof(int): return Unsafe.SizeOf<int>();
                case var t when t == typeof(long): return Unsafe.SizeOf<long>();
                case var t when t == typeof(ushort): return Unsafe.SizeOf<ushort>();
                case var t when t == typeof(uint): return Unsafe.SizeOf<uint>();
                case var t when t == typeof(ulong): return Unsafe.SizeOf<ulong>();
                case var t when t == typeof(sbyte): return Unsafe.SizeOf<sbyte>();
                case var t when t == typeof(byte): return Unsafe.SizeOf<byte>();
                case var t when t == typeof(char): return Unsafe.SizeOf<char>();
                case var t when t == typeof(bool): return Unsafe.SizeOf<bool>();
                case var t when t == typeof(Guid): return Unsafe.SizeOf<Guid>();
                case var t when t == typeof(TimeSpan): return Unsafe.SizeOf<TimeSpan>();
                case var t when t == typeof(DateTimeOffset): return Unsafe.SizeOf<DateTimeOffset>();
                case var t when t == typeof(DateTime): return Unsafe.SizeOf<DateTime>();
                default:
                    if (_typeSizes.ContainsKey(type)) return _typeSizes[type];
                    var method = new DynamicMethod("GetManagedSizeImpl", typeof(uint), new Type[0], typeof(TypeExtensions), false);
                    ILGenerator gen = method.GetILGenerator();
                    gen.Emit(OpCodes.Sizeof, type);
                    gen.Emit(OpCodes.Ret);
                    var size = checked((int)((Func<uint>)method.CreateDelegate(typeof(Func<uint>)))());
                    lock (_typeSizes)
                        _typeSizes.Add(type, size);
                    return size;
            };
        }
        private static Dictionary<Type, int> _typeSizes = new Dictionary<Type, int>();
       
    }