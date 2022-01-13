using System.Runtime.CompilerServices;

namespace HyperSerialize
{
    public struct Stacked<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stacked<T> Get(T val) => new Stacked<T> { Value = val };
        public T Value;
    };
}