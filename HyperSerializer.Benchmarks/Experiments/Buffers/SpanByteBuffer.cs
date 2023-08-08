using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET6_0_OR_GREATER
using BitOperations = System.Numerics.BitOperations;
#else
using BitOperations = HyperSerializer.Utilities.BitOperations;
#endif

namespace HyperSerializer.Utilities;

/// <summary>
/// Represents a heap-based, array-backed output sink into which <typeparamref name="T"/> data can be written.
/// </summary>
/// <typeparam name="T">The type of items to write to the current instance.</typeparam>
/// <remarks>
/// This is a custom <see cref="IBufferWriter{T}"/> implementation that replicates the
/// functionality and API surface of the array-based buffer writer available in
/// .NET Standard 2.1, with the main difference being the fact that in this case
/// the arrays in use are rented from the shared <see cref="ArrayPool{T}"/> instance,
/// and that <see cref="ArrayPoolBufferWriter{T}"/> is also available on .NET Standard 2.0.
/// </remarks>
public sealed class ArrayPoolBufferWriter<T> : IBuffer<T>, IMemoryOwner<T>
{
    /// <summary>
    /// The default buffer size to use to expand empty arrays.
    /// </summary>
    private const int DefaultInitialBufferSize = 256;

    /// <summary>
    /// The <see cref="ArrayPool{T}"/> instance used to rent <see cref="array"/>.
    /// </summary>
    private readonly ArrayPool<T> pool;

    /// <summary>
    /// The underlying <typeparamref name="T"/> array.
    /// </summary>
    private T[]? array;

#pragma warning disable IDE0032 // Use field over auto-property (clearer and faster)
    /// <summary>
    /// The starting offset within <see cref="array"/>.
    /// </summary>
    private int index;
#pragma warning restore IDE0032

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayPoolBufferWriter{T}"/> class.
    /// </summary>
    public ArrayPoolBufferWriter()
        : this(ArrayPool<T>.Shared, DefaultInitialBufferSize)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayPoolBufferWriter{T}"/> class.
    /// </summary>
    /// <param name="pool">The <see cref="ArrayPool{T}"/> instance to use.</param>
    public ArrayPoolBufferWriter(ArrayPool<T> pool)
        : this(pool, DefaultInitialBufferSize)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayPoolBufferWriter{T}"/> class.
    /// </summary>
    /// <param name="initialCapacity">The minimum capacity with which to initialize the underlying buffer.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="initialCapacity"/> is not valid.</exception>
    public ArrayPoolBufferWriter(int initialCapacity)
        : this(ArrayPool<T>.Shared, initialCapacity)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayPoolBufferWriter{T}"/> class.
    /// </summary>
    /// <param name="pool">The <see cref="ArrayPool{T}"/> instance to use.</param>
    /// <param name="initialCapacity">The minimum capacity with which to initialize the underlying buffer.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="initialCapacity"/> is not valid.</exception>
    public ArrayPoolBufferWriter(ArrayPool<T> pool, int initialCapacity)
    {
        // Since we're using pooled arrays, we can rent the buffer with the
        // default size immediately, we don't need to use lazy initialization
        // to save unnecessary memory allocations in this case.
        // Additionally, we don't need to manually throw the exception if
        // the requested size is not valid, as that'll be thrown automatically
        // by the array pool in use when we try to rent an array with that size.
        this.pool = pool;
        this.array = pool.Rent(initialCapacity);
        this.index = 0;
    }

    /// <inheritdoc/>
    Memory<T> IMemoryOwner<T>.Memory
    {
        // This property is explicitly implemented so that it's hidden
        // under normal usage, as the name could be confusing when
        // displayed besides WrittenMemory and GetMemory().
        // The IMemoryOwner<T> interface is implemented primarily
        // so that the AsStream() extension can be used on this type,
        // allowing users to first create a ArrayPoolBufferWriter<byte>
        // instance to write data to, then get a stream through the
        // extension and let it take care of returning the underlying
        // buffer to the shared pool when it's no longer necessary.
        // Inlining is not needed here since this will always be a callvirt.
        get => MemoryMarshal.AsMemory(WrittenMemory);
    }

    /// <inheritdoc/>
    public ReadOnlyMemory<T> WrittenMemory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            T[]? array = this.array;

            if (array is null)
            {
                ThrowObjectDisposedException();
            }

            return array!.AsMemory(0, this.index);
        }
    }

    /// <inheritdoc/>
    public ReadOnlySpan<T> WrittenSpan
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            T[]? array = this.array;

            if (array is null)
            {
                ThrowObjectDisposedException();
            }

            return array!.AsSpan(0, this.index);
        }
    }

    /// <inheritdoc/>
    public int WrittenCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.index;
    }

    /// <inheritdoc/>
    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            T[]? array = this.array;

            if (array is null)
            {
                ThrowObjectDisposedException();
            }

            return array!.Length;
        }
    }

    /// <inheritdoc/>
    public int FreeCapacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            T[]? array = this.array;

            if (array is null)
            {
                ThrowObjectDisposedException();
            }

            return array!.Length - this.index;
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        T[]? array = this.array;

        if (array is null)
        {
            ThrowObjectDisposedException();
        }

        array.AsSpan(0, this.index).Clear();

        this.index = 0;
    }

    /// <inheritdoc/>
    public void Advance(int count)
    {
        T[]? array = this.array;

        if (array is null)
        {
            ThrowObjectDisposedException();
        }

        if (count < 0)
        {
            ThrowArgumentOutOfRangeExceptionForNegativeCount();
        }

        if (this.index > array!.Length - count)
        {
            ThrowArgumentExceptionForAdvancedTooFar();
        }

        this.index += count;
    }

    /// <inheritdoc/>
    public Memory<T> GetMemory(int sizeHint = 0)
    {
        CheckBufferAndEnsureCapacity(sizeHint);

        return this.array.AsMemory(this.index);
    }

    /// <inheritdoc/>
    public Span<T> GetSpan(int sizeHint = 0)
    {
        CheckBufferAndEnsureCapacity(sizeHint);

        return this.array.AsSpan(this.index);
    }

    /// <summary>
    /// Ensures that <see cref="array"/> has enough free space to contain a given number of new items.
    /// </summary>
    /// <param name="sizeHint">The minimum number of items to ensure space for in <see cref="array"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckBufferAndEnsureCapacity(int sizeHint)
    {
        T[]? array = this.array;

        if (array is null)
        {
            ThrowObjectDisposedException();
        }

        if (sizeHint < 0)
        {
            ThrowArgumentOutOfRangeExceptionForNegativeSizeHint();
        }

        if (sizeHint == 0)
        {
            sizeHint = 1;
        }

        if (sizeHint > array!.Length - this.index)
        {
            ResizeBuffer(sizeHint);
        }
    }

    /// <summary>
    /// Resizes <see cref="array"/> to ensure it can fit the specified number of new items.
    /// </summary>
    /// <param name="sizeHint">The minimum number of items to ensure space for in <see cref="array"/>.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ResizeBuffer(int sizeHint)
    {
        uint minimumSize = (uint)this.index + (uint)sizeHint;

        // The ArrayPool<T> class has a maximum threshold of 1024 * 1024 for the maximum length of
        // pooled arrays, and once this is exceeded it will just allocate a new array every time
        // of exactly the requested size. In that case, we manually round up the requested size to
        // the nearest power of two, to ensure that repeated consecutive writes when the array in
        // use is bigger than that threshold don't end up causing a resize every single time.
        if (minimumSize > 1024 * 1024)
        {
            minimumSize = BitOperations.RoundUpToPowerOf2(minimumSize);
        }

        this.pool.Resize(ref this.array, (int)minimumSize);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        T[]? array = this.array;

        if (array is null)
        {
            return;
        }

        this.array = null;

        this.pool.Return(array);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        // See comments in MemoryOwner<T> about this
        if (typeof(T) == typeof(char) &&
            this.array is char[] chars)
        {
            return new(chars, 0, this.index);
        }

        // Same representation used in Span<T>
        return $"CommunityToolkit.HighPerformance.Buffers.ArrayPoolBufferWriter<{typeof(T)}>[{this.index}]";
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> when the requested count is negative.
    /// </summary>
    private static void ThrowArgumentOutOfRangeExceptionForNegativeCount()
    {
        throw new ArgumentOutOfRangeException("count", "The count can't be a negative value.");
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> when the size hint is negative.
    /// </summary>
    private static void ThrowArgumentOutOfRangeExceptionForNegativeSizeHint()
    {
        throw new ArgumentOutOfRangeException("sizeHint", "The size hint can't be a negative value.");
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> when the requested count is negative.
    /// </summary>
    private static void ThrowArgumentExceptionForAdvancedTooFar()
    {
        throw new ArgumentException("The buffer writer has advanced too far.");
    }

    /// <summary>
    /// Throws an <see cref="ObjectDisposedException"/> when <see cref="array"/> is <see langword="null"/>.
    /// </summary>
    private static void ThrowObjectDisposedException()
    {
        throw new ObjectDisposedException("The current buffer has already been disposed.");
    }
}
/// <summary>
/// An interface that expands <see cref="IBufferWriter{T}"/> with the ability to also inspect
/// the written data, and to reset the underlying buffer to write again from the start.
/// </summary>
/// <typeparam name="T">The type of items in the current buffer.</typeparam>
public interface IBuffer<T> : IBufferWriter<T>
{
    /// <summary>
    /// Gets the data written to the underlying buffer so far, as a <see cref="ReadOnlyMemory{T}"/>.
    /// </summary>
    ReadOnlyMemory<T> WrittenMemory { get; }

    /// <summary>
    /// Gets the data written to the underlying buffer so far, as a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    ReadOnlySpan<T> WrittenSpan { get; }

    /// <summary>
    /// Gets the amount of data written to the underlying buffer so far.
    /// </summary>
    int WrittenCount { get; }

    /// <summary>
    /// Gets the total amount of space within the underlying buffer.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// Gets the amount of space available that can still be written into without forcing the underlying buffer to grow.
    /// </summary>
    int FreeCapacity { get; }

    /// <summary>
    /// Clears the data written to the underlying buffer.
    /// </summary>
    /// <remarks>
    /// You must clear the <see cref="IBuffer{T}"/> instance before trying to re-use it.
    /// </remarks>
    void Clear();
}
/// <summary>
/// Utility methods for intrinsic bit-twiddling operations. The methods use hardware intrinsics
/// when available on the underlying platform, otherwise they use optimized software fallbacks.
/// </summary>
internal static class BitOperations
{
    /// <summary>
    /// Round the given integral value up to a power of 2.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    /// The smallest power of 2 which is greater than or equal to <paramref name="value"/>.
    /// If <paramref name="value"/> is 0 or the result overflows, returns 0.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint RoundUpToPowerOf2(uint value)
    {
        // Based on https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
        --value;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;

        return value + 1;
    }
}

/// <summary>
/// Helpers for working with the <see cref="ArrayPool{T}"/> type.
/// </summary>
public static class ArrayPoolExtensions
{
    /// <summary>
    /// Changes the number of elements of a rented one-dimensional array to the specified new size.
    /// </summary>
    /// <typeparam name="T">The type of items into the target array to resize.</typeparam>
    /// <param name="pool">The target <see cref="ArrayPool{T}"/> instance to use to resize the array.</param>
    /// <param name="array">The rented <typeparamref name="T"/> array to resize, or <see langword="null"/> to create a new array.</param>
    /// <param name="newSize">The size of the new array.</param>
    /// <param name="clearArray">Indicates whether the contents of the array should be cleared before reuse.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="newSize"/> is less than 0.</exception>
    /// <remarks>When this method returns, the caller must not use any references to the old array anymore.</remarks>
    public static void Resize<T>(this ArrayPool<T> pool, [NotNull] ref T[]? array, int newSize, bool clearArray = false)
    {
        // If the old array is null, just create a new one with the requested size
        if (array is null)
        {
            array = pool.Rent(newSize);

            return;
        }

        // If the new size is the same as the current size, do nothing
        if (array.Length == newSize)
        {
            return;
        }

        // Rent a new array with the specified size, and copy as many items from the current array
        // as possible to the new array. This mirrors the behavior of the Array.Resize API from
        // the BCL: if the new size is greater than the length of the current array, copy all the
        // items from the original array into the new one. Otherwise, copy as many items as possible,
        // until the new array is completely filled, and ignore the remaining items in the first array.
        T[] newArray = pool.Rent(newSize);
        int itemsToCopy = Math.Min(array.Length, newSize);

        Array.Copy(array, 0, newArray, 0, itemsToCopy);

        pool.Return(array, clearArray);

        array = newArray;
    }

    /// <summary>
    /// Ensures that when the method returns <paramref name="array"/> is not null and is at least <paramref name="capacity"/> in length.
    /// Contents of <paramref name="array"/> are not copied if a new array is rented.
    /// </summary>
    /// <typeparam name="T">The type of items into the target array given as input.</typeparam>
    /// <param name="pool">The target <see cref="ArrayPool{T}"/> instance used to rent and/or return the array.</param>
    /// <param name="array">The rented <typeparamref name="T"/> array to ensure capacity for, or <see langword="null"/> to rent a new array.</param>
    /// <param name="capacity">The minimum length of <paramref name="array"/> when the method returns.</param>
    /// <param name="clearArray">Indicates whether the contents of the array should be cleared if returned to the pool.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> is less than 0.</exception>
    /// <remarks>When this method returns, the caller must not use any references to the old array anymore.</remarks>
    public static void EnsureCapacity<T>(this ArrayPool<T> pool, [NotNull] ref T[]? array, int capacity, bool clearArray = false)
    {
        if (capacity < 0)
        {
            ThrowArgumentOutOfRangeExceptionForNegativeArrayCapacity();
        }

        if (array is null)
        {
            array = pool.Rent(capacity);
        }
        else if (array.Length < capacity)
        {
            // Ensure rent succeeds before returning the original array to the pool
            T[] newArray = pool.Rent(capacity);

            pool.Return(array, clearArray);

            array = newArray;
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> when the "capacity" parameter is negative.
    /// </summary>
    private static void ThrowArgumentOutOfRangeExceptionForNegativeArrayCapacity()
    {
        throw new ArgumentOutOfRangeException("capacity", "The array capacity must be a positive number.");
    }
}
   public ref struct SpanByteBuffer
{
	private byte[] _rental;
	private Span<byte> _buffer;
	private int _pos;

	public SpanByteBuffer(Span<byte> initialBuffer)
	{
		_rental = null;
		_buffer = initialBuffer;
		_pos = 0;
	}

	public SpanByteBuffer(int initialCapacity)
	{
		_rental = ArrayPool<byte>.Shared.Rent(initialCapacity);
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

	public ref byte GetPinnableReference(bool terminate = false)
	{
		if (terminate)
		{
			EnsureCapacity(Length + 1);
			_buffer[Length] = (byte)'\0';
		}
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


	public Span<byte> Buffer => _buffer;

	public ReadOnlySpan<byte> AsSpan(bool terminate = false)
	{
		if (terminate)
		{
			EnsureCapacity(Length + 1);
			_buffer[Length] = (byte)'\0';
		}
		return _buffer.Slice(0, _pos);
	}

	public ReadOnlySpan<byte> AsSpan() => _buffer.Slice(0, _pos);
	public ReadOnlySpan<byte> AsSpan(int start) => _buffer.Slice(start, _pos - start);
	public ReadOnlySpan<byte> AsSpan(int start, int length) => _buffer.Slice(start, length);

	public bool TryCopyTo(Span<byte> destination, out int count)
	{
		if (_buffer.Slice(0, _pos).TryCopyTo(destination))
		{
			count = _pos;
			Dispose();
			return true;
		}
		else
		{
			count = 0;
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
	public void Append(byte b)
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
	public void Append(byte[] b)
	{
		if (b == null)
		{
			return;
		}

		int pos = _pos;
		if (b.Length == 1 && (uint)pos < (uint)_buffer.Length)
		{
			_buffer[pos] = b[0];
			_pos = pos + 1;
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

	public unsafe void Append(byte* value, int length)
	{
		int pos = _pos;
		if (pos > _buffer.Length - length)
			Grow(length);

		Span<byte> dst = _buffer.Slice(_pos, length);
		for (int i = 0; i < dst.Length; i++) dst[i] = *value++;
			
		_pos += length;
	}
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(ReadOnlySpan<byte> value)
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
		Append(c);
	}

    private int growCount = 4;
    private double maxBuffer = 4;
        
	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Grow(int additionalCapacityBeyondPos)
	{
		Debug.Assert(additionalCapacityBeyondPos > 0);
		Debug.Assert(_pos > _buffer.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");
            
		byte[] poolArray = new byte[((int)Math.Max((uint)(_pos + additionalCapacityBeyondPos), (uint)_buffer.Length * 
            4
            ))];
        _buffer.Slice(0, _pos).CopyTo(poolArray);
        _buffer = _rental = poolArray;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose()
	{
		//byte[] toReturn = _rental;
		this = default;
		//if (toReturn != null)
		//	ArrayPool<byte>.Shared.Return(toReturn);
	}
}

