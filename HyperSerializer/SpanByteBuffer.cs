using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HyperSerializer
{
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
}
