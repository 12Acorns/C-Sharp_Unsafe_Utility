using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using CSPP.lib.std.allocator;
using CSPP.lib.std.ptr;
using System.Numerics;

namespace CSPP.lib.std.text;

internal readonly unsafe struct c_string<Allocator, AllocatorResult> : IDisposable
	where Allocator : unmanaged, IAllocator<AllocatorResult>
	where AllocatorResult : unmanaged, IAllocReturn
{
	private static readonly Vector256<ushort> _nullTerminatorVector = Vector256.Create((ushort)'\0');

	private readonly meta_pointer<char> _start;

	public c_string(Allocator* allocator, ReadOnlySpan<char> content)
	{
		var pad = GetPadding(content.Length + 1);
		_start = allocator->Allocate<char>(pad).Pointer.As<char>();
		_start.Pointer[content.Length] = '\0';
		_start.IsNative = false;
		if(content.Length > 0)
		{
			var copyTo = new Span<char>(_start.Pointer, content.Length);
			content.CopyTo(copyTo);
		}
	}
	public c_string(Allocator* allocator, int length)
	{
		if(length < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative.");
		}

		_start = allocator->Allocate<char>(GetPadding(length + 1)).Pointer.As<char>();

		_start.Pointer[length] = '\0';
		_start.IsNative = false;
	}
	public c_string(ReadOnlySpan<char> content)
	{
		_start = new meta_pointer<char>((char*)malloc(GetPadding(content.Length + 1) * sizeof(char)), true);
		_start.Pointer[content.Length] = '\0';
		if(content.Length > 0)
		{
			content.CopyTo(new Span<char>(_start.Pointer, content.Length));
		}
	}
	public c_string(int length)
	{
		if(length < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative.");
		}

		_start = new meta_pointer<char>((char*)malloc(GetPadding(length + 1) * sizeof(char)), true);
		_start.Pointer[length] = '\0';
	}
	public c_string()
	{
		_start = new meta_pointer<char>((char*)NULLPTR, false);
	}

	public readonly char* Start => _start.Pointer;
	public readonly bool IsEmpty => _start.Pointer == NULL || Length() > 0;
	public readonly bool IsDefault => _start.Pointer == NULL;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly int Length()
	{
		if(_start.Pointer == NULL)
		{
			return 0;
		}
		if(Vector256.IsHardwareAccelerated && Vector256<ushort>.IsSupported)
		{
			return LengthSIMD();
		}
		return Length(0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly char GetNoBoundsCheck(int indx) => Start[indx];
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly void SetNoBoundsCheck(int indx, char value) => Start[indx] = value;

	public char this[int idx]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			var len = Length();
			if(idx < 0 || idx >= len)
			{
				throw new IndexOutOfRangeException($"Index {idx} is out of range for c_string of length {len}.");
			}
			return Start[idx];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			var len = Length();
			if(idx < 0 || idx >= len)
			{
				throw new IndexOutOfRangeException($"Index {idx} is out of range for c_string of length {len}.");
			}
			Start[idx] = value;
		}
	}

	public readonly ReadOnlySpan<char> AsSpan() => IsDefault ? ReadOnlySpan<char>.Empty : new Span<char>(Start, Length());
	public readonly override string ToString() => AsSpan().ToString();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose()
	{
		if(_start.IsNative)
		{
			free(_start.Pointer);
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private readonly int Length(int length)
	{
		while(_start.Pointer[length] != '\0')
		{
			length++;
		}
		return length;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private readonly int LengthSIMD()
	{
		int length = 0;
		while(true)
		{
			var vec = Vector256.Create(new ReadOnlySpan<ushort>(_start.Pointer + length, Vector256<ushort>.Count));
			var cmp = Vector256.Equals(vec, _nullTerminatorVector);
			if(!cmp.Equals(Vector256<ushort>.Zero))
			{
				var mask = cmp.ExtractMostSignificantBits();
				var vecIndex = BitOperations.TrailingZeroCount(mask);
				return length + vecIndex;
			}
			length += Vector256<ushort>.Count;
		}
	}
	private static int GetPadding(int length)
	{
		var rem = length % Vector256<ushort>.Count;
		if(rem == 0)
		{
			return length;
		}
		return length + (Vector256<ushort>.Count - rem);
	}
}