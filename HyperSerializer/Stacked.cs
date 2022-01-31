using System.Runtime.CompilerServices;

namespace Hyper
{
    internal struct Stacked<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stacked<T> Get(T val) => new Stacked<T> { Value = val };
        public T Value;
    };
}