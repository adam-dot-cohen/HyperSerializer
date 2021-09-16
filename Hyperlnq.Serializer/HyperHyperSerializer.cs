using System;

namespace Hyperlnq.Serializer
{
    public class HyperHyperSerializer : IHyperSerializer
    {
        public T Deserialize<T>(ReadOnlySpan<byte> obj)
        {
            return HyperSerializer<T>.Deserialize(obj);
        }

        public void Serialize<T>(out Span<byte> bytes, T obj)
        {
            bytes = HyperSerializer<T>.Serialize(obj);
        }
        public T Deserialize<T>(ReadOnlyMemory<byte> obj)
        {
            return HyperSerializer<T>.Deserialize(obj.Span);
        }

        public void Serialize<T>(out Memory<byte> bytez, T obj)
        {
            bytez = HyperSerializer<T>.SerializeAsync(obj);
        }
    }
}