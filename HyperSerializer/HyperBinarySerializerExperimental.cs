using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace HyperSerialize
{
    internal static class HyperBinarySerializerExperimental
    {
        /// <summary>
        /// Serialize any value type, reference type or collection to Span{byte}
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<byte> Serialize<T>(T obj)
        {
            var s1 = Unsafe.SizeOf<T>();
            Span<byte> b1 = new byte[s1];
            fixed (byte* s2 = b1) Unsafe.Write(s2, obj);
            return b1;
        }
        //public static Span<byte> Serialize<T>(T obj)
        //{
        //    byte b = default;
        //    Unsafe.WriteUnaligned(ref b, Stacked<T>.Get(obj));
        //    var s1 = Unsafe.SizeOf<Stacked<T>>();
        //    return MemoryMarshal.CreateSpan(ref b, s1);
        //}
        //public static Span<byte> Serialize<T>(T obj)
        //{
        //    byte b = default;
        //    Unsafe.WriteUnaligned(ref b, obj);
        //    var s1 = Unsafe.SizeOf<T>();
        //    return MemoryMarshal.CreateSpan(ref b, s1);
        //}
        /// <summary>
        /// Deserialize binary obtained using the Serialiize functions of this class to to type T
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static unsafe T Deserialize<T>(ReadOnlySpan<byte> bytes)
        //{
        //    fixed (byte* s2 = bytes)
        //        return Unsafe.AsRef<T>(s2);
        //}
        public static unsafe T Deserialize<T>(ReadOnlySpan<byte> bytes)
        {
            fixed (byte* s2 = bytes)
                return Unsafe.Read<T>(s2);
        }
        /// <summary>
        /// Async serialize any value type, reference type or collection to Memory{byte}
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<Memory<byte>> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new ValueTask<Memory<byte>>(Task.FromCanceled<Memory<byte>>(cancellationToken));
            }
            return new ValueTask<Memory<byte>>(Serialize(obj).ToArray());
        }
        /// <summary>
        /// Async deserialize binary obtained using the Serialiize functions of this class to to type T
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<T> DeserializeAsync<T>(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new ValueTask<T>(Task.FromCanceled<T>(cancellationToken));
            }
            return new ValueTask<T>(Deserialize<T>(bytes.Span));
        }
        /// <summary>
        /// WARNING, NOT OPTIMIZED. For best performance, the fully optimized <see cref="Serialize(T)"/> and <see cref="SerializeAsync(T, CancellationToken)"/> methods
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize<T>(Stream stream, T value)
        {
            stream.Write(Serialize(value));
        }
        /// <summary>
        /// WARNING, NOT OPTIMIZED. For best performance, the fully optimized <see cref="Deserialize(ReadOnlySpan{byte})"/> and <see cref="DeserializeAsync(ReadOnlyMemory{byte}, CancellationToken)"/> methods
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static T Deserialize<T>(Stream stream)
        {
            Span<byte> sp = new byte[stream.Length];
            if (stream.Read(sp) != stream.Length)
            {
                throw new EndOfStreamException();
            }

            return Deserialize<T>(sp);
        }
        /// <summary>
        /// WARNING, NOT OPTIMIZED. For best performance, the fully optimized <see cref="Serialize(T)"/> and <see cref="SerializeAsync(T, CancellationToken)"/> methods
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask SerializeAsync<T>(Stream stream, T value, CancellationToken cancellationToken = default)
        {
            return WriteAsync(stream, SerializeAsync(value, cancellationToken).GetAwaiter().GetResult(), cancellationToken);
        }
        /// <summary>
        /// WARNING, NOT OPTIMIZED. For best performance, the fully optimized <see cref="Deserialize(ReadOnlySpan{byte})"/> and <see cref="DeserializeAsync(ReadOnlyMemory{byte}, CancellationToken)"/> methods
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
        {
            Memory<byte> mem = new byte[stream.Length];
            var len = ReadAsync(stream, mem, cancellationToken);
            return DeserializeAsync<T>(mem, cancellationToken);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ValueTask<int> ReadAsync(Stream stream, Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> segment))
            {
                return new ValueTask<int>(stream.ReadAsync(segment.Array!, segment.Offset, segment.Count, cancellationToken));
            }

            static async Task<int> ReadAsyncFallback(Stream stream, Memory<byte> buffer, CancellationToken cancellationToken)
            {
                byte[] rent = ArrayPool<byte>.Shared.Rent(buffer.Length);

                try
                {
                    int bytesRead = await stream.ReadAsync(rent, 0, buffer.Length, cancellationToken);

                    if (bytesRead > 0)
                    {
                        rent.AsSpan(0, bytesRead).CopyTo(buffer.Span);
                    }

                    return bytesRead;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(rent);
                }
            }

            return new ValueTask<int>(ReadAsyncFallback(stream, buffer, cancellationToken));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ValueTask WriteAsync(Stream stream, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> segment))
            {
                return new ValueTask(stream.WriteAsync(segment.Array!, segment.Offset, segment.Count, cancellationToken));
            }
            static async Task WriteAsyncFallback(Stream stream, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
            {
                byte[] rent = ArrayPool<byte>.Shared.Rent(buffer.Length);

                try
                {
                    buffer.Span.CopyTo(rent);

                    await stream.WriteAsync(rent, 0, buffer.Length, cancellationToken);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(rent);
                }
            }
            return new ValueTask(WriteAsyncFallback(stream, buffer, cancellationToken));
        }
    }
}