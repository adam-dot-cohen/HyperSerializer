
using System.Diagnostics.CodeAnalysis;
using static System.Runtime.CompilerServices.Unsafe;
using static System.Runtime.InteropServices.MemoryMarshal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using DotNext;
using DotNext.Buffers;
using DotNext.IO;
using DotNext.Numerics;
using Microsoft.IO;
using Intrinsics = System.Runtime.Intrinsics;
using System.Buffers;
using System.Diagnostics;
using BenchmarkDotNet.Jobs;
using System.IO.Pipelines;
using HyperSerializer.Utilities;

namespace HyperSerializer.Benchmarks.Experiments;

[SimpleJob(runStrategy: RunStrategy.Throughput, launchCount: 1, invocationCount: 1, runtimeMoniker: RuntimeMoniker.Net60)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MemoryDiagnoser]
public class MemoryStreamingBenchmark
{
    private static readonly RecyclableMemoryStreamManager manager = new();
    private readonly byte[] chunk = new byte[128];

    [Params(100, 1000, 10_000, 100_000, 1_000_000)]
    public int TotalCount;

    private void Write(Stream output)
    {
        for (int remaining = TotalCount, taken; remaining > 0; remaining -= taken)
        {
            taken = Math.Min(remaining, chunk.Length);
            output.Write(chunk, 0, taken);
        }
    }

    private void Write(ResizableSpanWriter<byte> output)
    {
        for (int remaining = TotalCount, taken; remaining > 0; remaining -= taken)
        {
            taken = Math.Min(remaining, chunk.Length);
            output.Write(chunk);
        }
    }
    private void Write(ResizablePipe output)
    {
        for (int remaining = TotalCount, taken; remaining > 0; remaining -= taken)
        {
            taken = Math.Min(remaining, chunk.Length);
            output.Write(chunk);
        }
    }
    private void Write(ResizableMemByte output)
    {
        for (int remaining = TotalCount, taken; remaining > 0; remaining -= taken)
        {
            taken = Math.Min(remaining, chunk.Length);
            output.Write(chunk);
        }
    }
    private void Write(MemoryBuffer output)
    {
        for (int remaining = TotalCount, taken; remaining > 0; remaining -= taken)
        {
            taken = Math.Min(remaining, chunk.Length);
            output.Write(chunk);
        }
    }
    private unsafe void Write(ArrayPoolBufferWriter<byte> output)
    {
        for (int remaining = TotalCount, taken; remaining > 0; remaining -= taken)
        {
            taken = Math.Min(remaining, chunk.Length);
            var span = output.GetSpan(taken);
            chunk[..taken].CopyTo(span);
        }
    }
    private unsafe void Write(SpanByteBuffer output)
    {
        for (int remaining = TotalCount, taken; remaining > 0; remaining -= taken)
        {
            taken = Math.Min(remaining, chunk.Length);
            output.Append(chunk);
        }
    }
    private unsafe void Write(GrowableSpan output)
    {
        for (int remaining = TotalCount, taken; remaining > 0; remaining -= taken)
        {
            taken = Math.Min(remaining, chunk.Length);
            output.Write(chunk);
        }
    }
    //private unsafe void Write(SpanBytePool output)
    //{
    //    for (int remaining = TotalCount, taken; remaining > 0; remaining -= taken)
    //    {
    //        taken = Math.Min(remaining, chunk.Length);
    //        fixed (byte* b = chunk)
    //            output.Write(b, taken);
    //    }
    //}
    //[Benchmark(Description = "MemoryStream", Baseline = true)]
    //public void WriteToMemoryStream()
    //{
    //    using var ms = new MemoryStream();
    //    Write(ms);
    //}
    [Benchmark(Description = "SpanByteBuffer")]
    public void WriteToSpanByteBuffer()
    {
        var sbb = new SpanByteBuffer();
        Write(sbb);
    }
    //[Benchmark(Description = "GrowableSpan")]
    //public void WriteToGrowableSpan()
    //{
    //    var gs = new GrowableSpan();
    //    Write(gs);
    //}
    //[Benchmark(Description = "SpanBytePool")]
    //public void WriteToSpanBytePool()
    //{
    //    var gs = new SpanBytePool();
    //    Write(gs);
    //}

    [Benchmark(Description = "ArrayPoolBufferWriter")]
    public void ArrayPoolBufferWriterInternal()
    {
        using ArrayPoolBufferWriter<byte> writer = new();
        Write(writer);
    }
    [Benchmark(Description = "ArrayPoolBufferWriterStream")]
    public void ArrayPoolBufferWriterStream()
    {
        using ArrayPoolBufferWriter<byte> writer = new();
        using var stream = writer.AsStream();
        Write(writer);
    }
    //[Benchmark(Description = "RecyclableMemoryStream")]
    //public void WriteToRecyclableMemoryStream()
    //{
    //    using var ms = manager.GetStream();
    //    Write(ms);
    //}

    //[Benchmark(Description = "SparseBufferWriter<byte>")]
    //public void WriteToSparseBuffer()
    //{
    //    using var buffer = new SparseBufferWriter<byte>(4096, SparseBufferGrowth.Linear);
    //    using var ms = buffer.AsStream(false);
    //    Write(ms);
    //}

    //[Benchmark(Description = "PooledArrayBufferWriter<byte>")]
    //public void WriteToGrowableBuffer()
    //{
    //    using var buffer = new PooledArrayBufferWriter<byte>();
    //    using var ms = buffer.AsStream();
    //    Write(ms);
    //}

    //[Benchmark(Description = "FileBufferingWriter")]
    //public void WriteToBufferingWriter()
    //{
    //    using var writer = new FileBufferingWriter(asyncIO: false);
    //    Write(writer);
    //}
    [Benchmark(Description = "ResizableSpanWriter")]
    public void ResizableSpanByte()
    {
        var i = sizeof(int);
        var writer = new ResizableSpanWriter<byte>();
        Write(writer);
    }
    //[Benchmark(Description = "ResizablePipe")]
    //public void ResizableSpanByte2()
    //{
    //    var i = sizeof(int);
    //    var writer = new ResizablePipe(8);
    //    Write(writer);
    //}
    [Benchmark(Description = "ResizableMemByte")]
    public void ResizableMemByte()
    {
        var i = sizeof(int);
        var writer = new ResizableMemByte();
        Write(writer);
    }
    //[Benchmark(Description = "MemoryBuffer")]
    //public void MemroyBuffer()
    //{
    //    var i = sizeof(int);
    //    var writer = new MemoryBuffer(256);
    //    Write(writer);
    //}

}


public ref struct ResizablePipe
{
    private int _increment = 1;
    private byte[] _array = default;
    private Span<byte> _buffer;
    private int _offset = 0;
    private static ArrayPool<byte> _pool;
    private PipeWriter _writer;
    private MemoryStream _stream;
    public ResizablePipe(int initialLength = 256)
    {
        _stream = new MemoryStream();
        _writer = PipeWriter.Create(_stream);
        _increment = initialLength;
        _pool = ArrayPool<byte>.Shared;
        _buffer = _pool.Rent(initialLength);
        _offset = 0;
    }

    public Span<byte> Get() => _stream.GetBuffer();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Span<byte> bytes)
    {
        _writer.Write(bytes);
    }

}
//public struct VectorBuffer<T>
//    where T : unmanaged
//{
//    private int _rowLenInit = 1;
//    private int _rowLenInit = 1024;
//    private int _rowLen
//    private int _written = ;
//    private int _colWritten = 0;
//    private BitVector[] _buffer;
//    private int _offset = 0;
//    public ResizableSpanWriter(init)
//    {
//        _buffer = new Memory<T>[initialLength];
//        _length = 0;
//        _offset = 0;
//    }
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private void Grow()
//    {
//        Span<byte> next = new byte[_buffer.Length + _colSize];
//        _buffer.CopyTo(next);
//        _buffer = next;
//    }
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public Span<byte> Slice(int length)
//    {
//        if (_buffer.Length - _offset < _offset + length)
//            Grow();
//        var slice = _buffer.Slice(_offset, length);
//        _offset += length;
//        return slice;
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private static void GetBits<TValue, TVector>(TValue value, Span<bool> bits)
//        where TValue : unmanaged
//        where TVector : struct, IBitVector<TValue>
//    {
//        var vector = new TVector { Value = value };

//        for (var position = 0; position < bits.Length; position++)
//            bits[position] = vector[position];
//    }

//    private interface IBitVector<TValue>
//        where TValue : unmanaged
//    {
//        bool this[int position] { get; set; }

//        TValue Value { get; set; }
//    }

//    [StructLayout(LayoutKind.Auto)]
//    private struct UInt32Vector : IBitVector<uint>
//    {
//        private uint result;

//        bool IBitVector<uint>.this[int position]
//        {
//            readonly get => ((result >> position) & 1U) != 0U;
//            set => result = (result & ~(1U << position)) | ((uint)value.ToInt32() << position);
//        }

//        uint IBitVector<uint>.Value
//        {
//            readonly get => result;
//            set => result = value;
//        }
//    }

//    [StructLayout(LayoutKind.Auto)]
//    private struct UInt64Vector : IBitVector<ulong>
//    {
//        private ulong result;

//        bool IBitVector<ulong>.this[int position]
//        {
//            readonly get => ((result >> position) & 1UL) != 0UL;
//            set => result = (result & ~(1UL << position)) | ((ulong)value.ToInt32() << position);
//        }

//        ulong IBitVector<ulong>.Value
//        {
//            readonly get => result;
//            set => result = value;
//        }
//    }
//}

//public class PipeBuffer
//{
//    public PipeBuffer()
//    {
//    }
//    public 
//}
ref struct GrowableSpan
{
    private byte[] _rental;
    private Span<byte> _buffer;
    private int _pos;

    public GrowableSpan(Span<byte> initialBuffer)
    {
        _rental = null;
        _buffer = initialBuffer;
        _pos = 0;
    }

    public GrowableSpan(int initialCapacity)
    {
        _rental = new byte[initialCapacity];
        _buffer = _rental;
        _pos = 0;
    }

    public int Length
    {
        get => _pos;
        set
        {
            Debug.Assert(value >= 0);
            Debug.Assert(value <= _buffer.Length);
            _pos = value;
        }
    }

    public int Capacity => _buffer.Length;

    public void EnsureCapacity(int capacity)
    {
        Debug.Assert(capacity >= 0);

        if ((uint)capacity > (uint)_buffer.Length)
            Grow(capacity - _pos);
    }

    public ref byte GetPinnableReference()
    {
        return ref MemoryMarshal.GetReference(_buffer);
    }

    public ref byte this[int index]
    {
        get
        {
            Debug.Assert(index < _pos);
            return ref _buffer[index];
        }
    }

    public override string ToString()
    {
        string s = _buffer.Slice(0, _pos).ToString();
        Dispose();
        return s;
    }

    public ReadOnlySpan<byte> AsSpan() => _buffer.Slice(0, _pos);
    public ReadOnlySpan<byte> AsSpan(int start) => _buffer.Slice(start, _pos - start);
    public ReadOnlySpan<byte> AsSpan(int start, int length) => _buffer.Slice(start, length);

    public bool TryCopyTo(Span<byte> destination, out int len)
    {
        if (_buffer.Slice(0, _pos).TryCopyTo(destination))
        {
            len = _pos;
            Dispose();
            return true;
        }
        else
        {
            len = 0;
            Dispose();
            return false;
        }
    }

    public void Insert(int index, byte value, int count)
    {
        if (_pos > _buffer.Length - count)
            Grow(count);

        int remaining = _pos - index;
        _buffer.Slice(index, remaining).CopyTo(_buffer.Slice(index + count));
        _buffer.Slice(index, count).Fill(value);
        _pos += count;
    }

    public void Insert(int index, byte[] s)
    {
        if (s == null) return;

        int count = s.Length;

        if (_pos > (_buffer.Length - count))
            Grow(count);

        int remaining = _pos - index;
        _buffer.Slice(index, remaining).CopyTo(_buffer.Slice(index + count));
        s
#if NET6_0_OR_GREATER
			.AsSpan()
#endif
            .CopyTo(_buffer.Slice(index));
        _pos += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte b)
    {
        int pos = _pos;
        if ((uint)pos < (uint)_buffer.Length)
        {
            _buffer[pos] = b;
            _pos = pos + 1;
        }
        else
            GrowAndAppend(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte[] b)
    {
        if (b == null)
        {
            return;
        }

        int pos = _pos;
        if (b.Length == 1 && (uint)pos < (uint)_buffer.Length)
        {
            _buffer[pos] = b[0];
            _pos++;
        }
        else
        {
            AppendSlow(b);
        }
    }

    private void AppendSlow(byte[] b)
    {
        int pos = _pos;
        if (pos > _buffer.Length - b.Length)
        {
            Grow(b.Length);
        }

        b
#if !NET6_0_OR_GREATER
			.AsSpan()
#endif
            .CopyTo(_buffer.Slice(pos));
        _pos += b.Length;
    }

    public void Append(byte b, int count)
    {
        if (_pos > _buffer.Length - count)
            Grow(count);

        Span<byte> dst = _buffer.Slice(_pos, count);
        for (int i = 0; i < dst.Length; i++) dst[i] = b;

        _pos += count;
    }

    public unsafe void Write(byte* value, int length)
    {
        int pos = _pos;
        if (pos > _buffer.Length - length)
            Grow(length);

        Span<byte> dst = _buffer.Slice(_pos, length);
        for (int i = 0; i < dst.Length; i++) dst[i] = *value++;

        _pos += length;
    }

    public void Write(ReadOnlySpan<byte> value)
    {
        int pos = _pos;
        if (pos > _buffer.Length - value.Length) Grow(value.Length);

        value.CopyTo(_buffer.Slice(_pos));
        _pos += value.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> AppendSpan(int length)
    {
        int origPos = _pos;
        if (origPos > _buffer.Length - length) Grow(length);

        _pos = origPos + length;
        return _buffer.Slice(origPos, length);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowAndAppend(byte c)
    {
        Grow(1);
        Write(c);
    }

    /// <summary>
    /// Resize the internal buffer either by doubling current buffer size or
    /// by adding <paramref name="additionalCapacityBeyondPos"/> to
    /// <see cref="_pos"/> whichever is greater.
    /// </summary>
    /// <param name="additionalCapacityBeyondPos">
    /// Number of chars requested beyond current position.
    /// </param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int additionalCapacityBeyondPos)
    {
        Debug.Assert(additionalCapacityBeyondPos > 0);
        Debug.Assert(_pos > _buffer.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

        // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative
        byte[] poolArray = new byte[((int)Math.Max((uint)(_pos + additionalCapacityBeyondPos), (uint)_buffer.Length * 2))];

        _buffer.Slice(0, _pos).CopyTo(poolArray);
        _buffer = _rental = poolArray;

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        this = default;
    }
}
ref struct SpanBytePool
{
    private IMemoryOwner<byte> _rental;
    private Span<byte> _buffer;
    private int _pos;

    public SpanBytePool(Span<byte> initialBuffer)
    {
        _rental = null;
        _buffer = initialBuffer;
        _pos = 0;
    }

    public SpanBytePool(int initialCapacity)
    {
        _rental = MemoryPool<byte>.Shared.Rent(initialCapacity);
        _buffer = _rental.Memory.Span;
        _pos = 0;
    }

    public int Length
    {
        get => _pos;
        set
        {
            Debug.Assert(value >= 0);
            Debug.Assert(value <= _buffer.Length);
            _pos = value;
        }
    }

    public int Capacity => _buffer.Length;

    public void EnsureCapacity(int capacity)
    {
        Debug.Assert(capacity >= 0);

        if ((uint)capacity > (uint)_buffer.Length)
            Grow(capacity - _pos);
    }

    public ref byte GetPinnableReference()
    {
        return ref MemoryMarshal.GetReference(_buffer);
    }

    public ref byte this[int index]
    {
        get
        {
            Debug.Assert(index < _pos);
            return ref _buffer[index];
        }
    }

    public override string ToString()
    {
        string s = _buffer.Slice(0, _pos).ToString();
        Dispose();
        return s;
    }

    public ReadOnlySpan<byte> AsSpan() => _buffer.Slice(0, _pos);
    public ReadOnlySpan<byte> AsSpan(int start) => _buffer.Slice(start, _pos - start);
    public ReadOnlySpan<byte> AsSpan(int start, int length) => _buffer.Slice(start, length);

    public bool TryCopyTo(Span<byte> destination, out int len)
    {
        if (_buffer.Slice(0, _pos).TryCopyTo(destination))
        {
            len = _pos;
            Dispose();
            return true;
        }
        else
        {
            len = 0;
            Dispose();
            return false;
        }
    }

    public void Insert(int index, byte value, int count)
    {
        if (_pos > _buffer.Length - count)
            Grow(count);

        int remaining = _pos - index;
        _buffer.Slice(index, remaining).CopyTo(_buffer.Slice(index + count));
        _buffer.Slice(index, count).Fill(value);
        _pos += count;
    }

    public void Insert(int index, byte[] s)
    {
        if (s == null) return;

        int count = s.Length;

        if (_pos > (_buffer.Length - count))
            Grow(count);

        int remaining = _pos - index;
        _buffer.Slice(index, remaining).CopyTo(_buffer.Slice(index + count));
        s
#if !NET6_0_OR_GREATER
			.AsSpan()
#endif
            .CopyTo(_buffer.Slice(index));
        _pos += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte b)
    {
        int pos = _pos;
        if ((uint)pos < (uint)_buffer.Length)
        {
            _buffer[pos] = b;
            _pos = pos + 1;
        }
        else
            GrowAndAppend(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte[] b)
    {
        if (b == null)
        {
            return;
        }

        int pos = _pos;
        if (b.Length == 1 && (uint)pos < (uint)_buffer.Length)
        {
            _buffer[pos] = b[0];
            _pos++;
        }
        else
        {
            AppendSlow(b);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendSlow(byte[] b)
    {
        int pos = _pos;
        if (pos > _buffer.Length - b.Length)
        {
            Grow(b.Length);
        }

        b
#if !NET6_0_OR_GREATER
			.AsSpan()
#endif
            .CopyTo(_buffer.Slice(pos));
        _pos += b.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(byte b, int count)
    {
        if (_pos > _buffer.Length - count)
            Grow(count);

        Span<byte> dst = _buffer.Slice(_pos, count);
        for (int i = 0; i < dst.Length; i++) dst[i] = b;

        _pos += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void Write(byte* value, int length)
    {
        int pos = _pos;
        if (pos > _buffer.Length - length)
            Grow(length);

        Span<byte> dst = _buffer.Slice(_pos, length);
        for (int i = 0; i < dst.Length; i++) dst[i] = *value++;

        _pos += length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ReadOnlySpan<byte> value)
    {
        int pos = _pos;
        if (pos > _buffer.Length - value.Length) Grow(value.Length);

        value.CopyTo(_buffer.Slice(_pos));
        _pos += value.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> AppendSpan(int length)
    {
        int origPos = _pos;
        if (origPos > _buffer.Length - length) Grow(length);

        _pos = origPos + length;
        return _buffer.Slice(origPos, length);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowAndAppend(byte c)
    {
        Grow(1);
        Write(c);
    }

    /// <summary>
    /// Resize the internal buffer either by doubling current buffer size or
    /// by adding <paramref name="additionalCapacityBeyondPos"/> to
    /// <see cref="_pos"/> whichever is greater.
    /// </summary>
    /// <param name="additionalCapacityBeyondPos">
    /// Number of chars requested beyond current position.
    /// </param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int additionalCapacityBeyondPos)
    {
        Debug.Assert(additionalCapacityBeyondPos > 0);
        Debug.Assert(_pos > _buffer.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

        // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative

        IMemoryOwner<byte> poolArray = MemoryPool<byte>.Shared.Rent((int)Math.Max((uint)(_pos + additionalCapacityBeyondPos), (uint)_buffer.Length * 2));

        _buffer.Slice(0, _pos).CopyTo(poolArray.Memory.Span);

        IMemoryOwner<byte> toReturn = _rental;
        _rental = poolArray;
        _buffer = _rental.Memory.Span;
        if (toReturn != null)
            toReturn.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        IMemoryOwner<byte> toReturn = _rental;
        this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
        if (toReturn != null)
            toReturn.Dispose();
    }
}

public class SpanByteMemoryPool : IDisposable
{
    private int _increment = 1;
    private MemoryPool<byte> _pool;
    private IMemoryOwner<byte> _owner;
    private int _offset = 0;

    public SpanByteMemoryPool(int initialLength = 1024)
    {
        _increment = initialLength;
        _pool = MemoryPool<byte>.Shared;
        _owner = _pool.Rent(initialLength);
        _offset = 0;
    }

    public Span<byte> Get() => _owner.Memory.Span.Slice(0, _offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Span<byte> bytes)
    {
        var slice = Slice(bytes.Length);
        slice = bytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Grow(int length)
    {
        var len = Math.Max(_offset + _increment, _offset + length);
        if (len > _pool.MaxBufferSize) throw new Exception("Max buffer size exceeded.");
        var owner = _pool.Rent(len);
        var memcopy = owner.Memory.Span.Slice(0, length);
        memcopy = _owner.Memory.Span;
        _owner.Dispose();
        _owner = owner;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> Slice(int length)
    {
        if (_offset + length > _owner.Memory.Length)
            Grow(length);

        var slc = _owner.Memory.Span.Slice(_offset, length);
        _offset += length;
        return slc;
    }

    public void Dispose()
    {
        _owner?.Dispose();
    }
}

public class ResizableSpanWriter<T> where T : struct
{
    private T[] _last;
    private int _increment = 1;
    private Memory<T> _buffer;
    private int _offset = 0;
    public ResizableSpanWriter(int initialLength = 1024)
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            throw new NotSupportedException(
                "ResizableSpanWriter only supports value types that are compatible with Span<T>");
        _increment = initialLength;
        _buffer = _last = ArrayPool<T>.Shared.Rent(initialLength);
        _offset = 0;
    }

    public Span<T> Get() => _buffer.Span.Slice(0, _offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Span<T> items)
    {
        var slice = Slice(items.Length);
        slice = items;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(T item)
    {
        var slice = Slice(1);
        slice[0]= item;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Grow(int length)
    {
        if (_offset + length <= _buffer.Span.Length) return;
		
        var next = ArrayPool<T>.Shared.Rent(Math.Max(_offset + _increment, _offset + length));

        _buffer.Span.CopyTo(next);

        ArrayPool<T>.Shared.Return(_last);

        _buffer = _last = next;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> Slice(int length)
    {
        Grow(length);

        var slc = _buffer.Span.Slice(_offset, length);
     
        _offset += length;
        
        return slc;
    }
}
public struct ResizableMemByte
{
    private int _increment = 1;
    private Memory<byte> _buffer;
    private int _offset = 0;
    public ResizableMemByte(int initialLength = 1024)
    {
        _increment = initialLength;
        _buffer = new byte[initialLength];
        _offset = 0;
    }

    public Memory<byte> Get() => _buffer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Memory<byte> bytes)
    {
        bytes.CopyTo(Slice(bytes.Length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Grow(int length)
    {
        Memory<byte> next = ArrayPool<byte>.Shared.Rent(Math.Max(_offset + _increment, _offset + length));
        _buffer.CopyTo(next);
        _buffer = next;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Memory<byte> Slice(int length)
    {
        if (_offset + length > _buffer.Length)
            Grow(length);

        var slc = _buffer.Slice(_offset, length);
        _offset += length;
        return slc;
    }
}

public ref struct MemoryBuffer
{
    private int _increment = 512;
    private Memory<byte> _buffer = default;
    private List<Memory<byte>> _buffers = new();
    private int _offset = 0;
    public MemoryBuffer(int initialLength = 512)
    {
        _increment = initialLength;
        Grow();
    }

    public Memory<byte> Get() => _buffer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Memory<byte> bytes)
    {
        var written = 0;
        while (written < bytes.Length)
        {
            var write = Math.Min(_increment - _offset, bytes.Length - written);
            var src = bytes.Slice(written, write);
            var dst = Slice(write);
            src.CopyTo(dst);
            written += write;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Grow()
    {
        _offset = 0;
        _buffer = ArrayPool<byte>.Shared.Rent(_increment);
        _buffers.Add(_buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Memory<byte> Slice(int length)
    {
        var remaining = _increment - _offset;
        if (_offset + length > remaining)
        {
            Grow();
        }
        var slc = _buffer.Slice(_offset, remaining);
        _offset += remaining;
        return slc;
    }
}