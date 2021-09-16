using System;

namespace Hyperlnq.Serializer
{

    public interface IHyperSerializer
    {
        void Serialize<T>(out Memory<byte> bytes, T obj);
        T Deserialize<T>(ReadOnlyMemory<byte> obj);
        void Serialize<T>(out Span<byte> bytes, T obj);
        T Deserialize<T>(ReadOnlySpan<byte> obj);
    }
}
