using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HyperSerialize
{
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

        public static string GetPropertyName<T>(this T val, Expression<Func<T>> propertyExpression) => (propertyExpression.Body as MemberExpression).Member.Name;

        private static ConcurrentDictionary<Type, int> _typeSizes = new ConcurrentDictionary<Type, int>();
        public static int SizeOf<TType>() => typeof(TType).SizeOf();
        public static int SizeOf(this Type type)
        {
            if (_typeSizes.ContainsKey(type)) return _typeSizes[type];
            var method = new DynamicMethod("GetManagedSizeImpl", typeof(uint), new Type[0], typeof(TypeExtensions), false);
            ILGenerator gen = method.GetILGenerator();
            gen.Emit(OpCodes.Sizeof, type);
            gen.Emit(OpCodes.Ret);
            var size = checked((int)((Func<uint>)method.CreateDelegate(typeof(Func<uint>)))());
            _typeSizes.TryAdd(type, size);
            return size;
        }
    }
}