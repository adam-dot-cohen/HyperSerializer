using System;
using MessagePack;

namespace Hyperlnq.Serializer.Benchmarks.Utilities
{
    public class MessagePackHyperSerializer : IHyperSerializer
    {
        public T Deserialize<T>(ReadOnlyMemory<byte> obj)
        {
            return MessagePackSerializer.Deserialize<T>(obj);
        }

        public void Serialize<T>(out Memory<byte> bytes, T obj)
        {
            bytes = MessagePackSerializer.Serialize(obj);
        }
        public T Deserialize<T>(ReadOnlySpan<byte> obj)
        {
            return MessagePackSerializer.Deserialize<T>(obj.ToArray());
        }

        public void Serialize<T>(out Span<byte> bytes, T obj)
        {
            bytes = MessagePackSerializer.Serialize(obj);
        }
    }
}