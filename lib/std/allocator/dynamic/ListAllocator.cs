using System.Runtime.InteropServices;
using System.Diagnostics;
using CSPP.lib.std.ptr;
using CSPP.lib.util;

namespace CSPP.lib.std.allocator.dynamic;

[StructLayout(LayoutKind.Sequential)]
internal unsafe partial record struct ListAllocator : IAllocator<ListNode>, IDisposable
{
	private const double DefaultGrowthFactor = 1.5d;
	private const int DefaultCapacity = 16;

	private meta_pointer<ListNode> _startAddress;
	private int _count;
	private int _size;

	public readonly int Capacity => _size;
	public readonly int Count => _count;
	 
	public ListNode Allocate<T>(int length) where T : unmanaged
	{
		if(length <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero.");
		}
		if(_count < _size)
		{
			InitNode<T>(_count, length);
			_count++;
		}
		else
		{
			AddWithResize<T>(length);
		}
		return _startAddress.Pointer[_count - 1];
	}

	public void Dispose()
	{
		for(int i = 0; i < _count; i++)
		{
			var node = _startAddress.Pointer[i];
			var nodePtr = node.Pointer.Pointer;
			free(node.Pointer.Pointer);
		}
		if(_startAddress.IsNative)
		{
			free(_startAddress.Pointer);
		}
	}

	private void AddWithResize<T>(int length) where T : unmanaged
	{
		Debug.Assert(_count == _size);

		_size = CalculateNewCapacity(_size + 1);
		Grow();
		if(_count + 1 < _size)
		{
			InitNode<T>(_count, length);
			_count++;
			return;
		}
		throw new InvalidOperationException("Failed to regrow the allocator.");
	}
	private void Grow()
	{
		// Realloc frees original buffer, and copies the content to the new buffer
		_startAddress.Pointer = (ListNode*)realloc((nint)_startAddress.Pointer, _size * sizeof(IntPtr));
	}
	private readonly int CalculateNewCapacity(int capacity)
	{
		Debug.Assert(_size < capacity);

		var newCapacity = (capacity == 0) ? DefaultCapacity : (int)Math.Ceiling(capacity * DefaultGrowthFactor);
		if(newCapacity > Array.MaxLength)
		{
			newCapacity = Array.MaxLength;
		}
		if(newCapacity < capacity)
		{
			newCapacity = capacity;
		}
		return newCapacity;
	}

	public readonly ReadOnlySpan<ListNode> this[Range range] => new ReadOnlySpan<ListNode>(_startAddress.Pointer, _size)[range];
	public readonly ListNode this[int index] => index < 0 || index >= _count
			? throw new IndexOutOfRangeException($"Index {index} is out of range. Valid range: 0 (inclusive) to {_count} (exclusive)")
			: _startAddress.Pointer[index];

	public static ListAllocator* Create(int capacity)
	{
		if(capacity < 0)
		{
			throw new ArgumentException($"Invalid capacity: {capacity}. Capacity must be greater than or equal to 0.");
		}
		var allocator = MemoryUtility.AllocStructureUnmanaged<ListAllocator>();
		allocator->_startAddress = new meta_pointer<ListNode>((ListNode*)malloc(capacity * PTR_SIZE), true);
		allocator->_size = capacity;
		allocator->_count = 0;
		return allocator;
	}
	public static ListAllocator* Create() => Create(0);
	private void InitNode<T>(int idx, int length) where T : unmanaged => ListNode.Init(dst: &_startAddress.Pointer[idx],
			length * sizeof(T), new((void*)malloc(length * sizeof(T)), true));
}
