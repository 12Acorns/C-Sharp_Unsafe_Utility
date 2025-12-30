using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CSPP.lib.std.ptr;

namespace CSPP.lib.std.allocator.arena;

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct ArenaAllocator : IAllocator<ArenaReference>, IDisposable
{
	private static readonly int _size = sizeof(ArenaAllocator);

	[FieldOffset(0)]
	private readonly nint _start;
	[FieldOffset(8)]
	private nint _currentOffset;
	[FieldOffset(16)]
	private readonly nint _lengthInBytes;
	[FieldOffset(24)]
	private readonly AllocatorDisposalMode _disposalMode;

	public readonly ulong AvailableBytes => (ulong)(_lengthInBytes - (_currentOffset - _start));

	/// <returns><see cref="ArenaReference{T}"/></returns>
	public ArenaReference Allocate<T>(int length) where T : unmanaged
	{
		nint need;
		try
		{
			checked
			{
				need = sizeof(T) * length;
			}
		}
		catch(OverflowException)
		{
			return default;
		}

		var used = _currentOffset - _start;
		if(used + need > _lengthInBytes || length <= 0)
		{
			return default;
		}
		var reference = new ArenaReference(_start, _currentOffset, length, sizeof(T));
		_currentOffset += length * sizeof(T);
		return reference;
	}

	public readonly void Dispose()
	{
		if(_disposalMode != AllocatorDisposalMode.NoClear)
		{
			if(_disposalMode == AllocatorDisposalMode.ClearWhole)
			{
				var region = new Span<byte>((byte*)_start, (int)_lengthInBytes);
				region.Clear();
			}
			else if(_currentOffset > _start)
			{
				var region = new Span<byte>((byte*)_start, (int)(_currentOffset - _start));
				region.Clear();
			}
		}
		free(_start);
	}

	public static unsafe meta_pointer<ArenaAllocator> CreateUnmanaged<T>(nint length, AllocatorDisposalMode disposalMode = AllocatorDisposalMode.NoClear) where T : unmanaged => 
		CreateUnTrackedByGC(length, sizeof(T), disposalMode);
	public static unsafe meta_pointer<ArenaAllocator> Create<T>(nint length, AllocatorDisposalMode disposalMode = AllocatorDisposalMode.NoClear) => 
		CreateUnTrackedByGC(length, Unsafe.SizeOf<T>(), disposalMode);

	private static meta_pointer<ArenaAllocator> CreateUnTrackedByGC(nint length, int size, AllocatorDisposalMode disposalMode = AllocatorDisposalMode.NoClear)
	{
		if(length <= 0 || size <= 0)
		{
			throw new ArgumentOutOfRangeException("Length and size must be greater than zero.");
		}

		nint byteCount;
		try
		{
			checked
			{
				byteCount = size * length;
			}
		}
		catch(OverflowException ex)
		{
			throw new OutOfMemoryException("Requested arena size exceeds addressable memory.", ex);
		}

		var arenaPtr = malloc(_size);
		if(arenaPtr == NULLPTR)
		{
			throw new OutOfMemoryException("Failed to allocate memory for Arena.");
		}

		var dataPtr = malloc(byteCount);
		if(dataPtr == NULLPTR)
		{
			free(arenaPtr);
			throw new OutOfMemoryException("Failed to allocate memory for Arena data.");
		}
		var arena = (ArenaAllocator*)arenaPtr;

		Unsafe.AsRef(in arena->_start) = dataPtr;
		Unsafe.AsRef(in arena->_currentOffset) = dataPtr;
		Unsafe.AsRef(in arena->_lengthInBytes) = byteCount;
		Unsafe.AsRef(in arena->_disposalMode) = disposalMode;
		return new meta_pointer<ArenaAllocator>((ArenaAllocator*)arenaPtr, true);
	}
}
