using CSPP.lib.util.Enumeration;
using CSPP.lib.std.allocator;
using System.Collections;
using CSPP.lib.std.ptr;

namespace CSPP.lib.std.STL.STL_98.List;

internal unsafe record struct Unmanaged_List<T, TAllocator, TAllocReturn> : IDisposable, IEnumerable<T>
	where T : unmanaged
	where TAllocator : unmanaged, IAllocator<TAllocReturn>
	where TAllocReturn : IAllocReturn
{
	private meta_pointer<TAllocator> _allocator;

	internal meta_pointer<Unmanaged_ListNode<T>> _head;
	internal meta_pointer<Unmanaged_ListNode<T>> _tail;

	public Unmanaged_List() { }

	public int Count { get; private set; }
	public readonly meta_pointer<Unmanaged_ListNode<T>> Start => _head.Pointer->Next;
	public readonly meta_pointer<Unmanaged_ListNode<T>> End => _tail.Pointer->Previous;

	public static meta_pointer<Unmanaged_List<T, TAllocator, TAllocReturn>> CreateUnmanaged(meta_pointer<TAllocator> allocator)
	{
		var buffer = allocator.Pointer->Allocate<Unmanaged_List<T, TAllocator, TAllocReturn>>(1).Pointer
			.As<Unmanaged_List<T, TAllocator, TAllocReturn>>();
		if(buffer.Pointer == NULL)
		{
			throw new OutOfMemoryException();
		}
		buffer.Pointer->Count = 0;
		buffer.Pointer->_allocator = allocator;
		buffer.Pointer->_head = new(Unmanaged_ListNode<T>.CreateEmpty<TAllocator, TAllocReturn>(allocator), true);
		buffer.Pointer->_tail = new(Unmanaged_ListNode<T>.CreateEmpty<TAllocator, TAllocReturn>(allocator), true);

		var headPtr = buffer.Pointer->_head.Pointer;
		headPtr->Next = buffer.Pointer->_tail;
		headPtr->Previous = new meta_pointer<Unmanaged_ListNode<T>>();

		var tailPtr = buffer.Pointer->_tail.Pointer;
		tailPtr->Next = new meta_pointer<Unmanaged_ListNode<T>>();
		tailPtr->Previous = buffer.Pointer->_head;

		return buffer;
	}

	public void InsertSort(T newNode)
	{
		var toAdd = Unmanaged_ListNode<T>.CreateNode<TAllocator, TAllocReturn>(newNode, _allocator);
		var next = _head.Pointer->Next;
		if(newNode is IComparable<T> comparable)
		{
			while(next != _tail && comparable.CompareTo(next.Pointer->Value) > 0)
			{
				next = next.Pointer->Next;
			}
		}
		else
		{
			while(next != _tail)
			{
				next = next.Pointer->Next;
			}
		}
		next.Pointer->Previous.Pointer->Next = new meta_pointer<Unmanaged_ListNode<T>>(toAdd, true);
		toAdd.Pointer->Next = next;
		toAdd.Pointer->Previous = next.Pointer->Previous;
		next.Pointer->Previous = new meta_pointer<Unmanaged_ListNode<T>>(toAdd, true);
		Count++;
	}
	public void InsertStart(T newNode)
	{
		var toAdd = Unmanaged_ListNode<T>.CreateNode<TAllocator, TAllocReturn>(newNode, _allocator);
		var next = _head.Pointer->Next;
		_head.Pointer->Next = new meta_pointer<Unmanaged_ListNode<T>>(toAdd, true);
		toAdd.Pointer->Previous = _head;
		toAdd.Pointer->Next = next;
		next.Pointer->Previous = new meta_pointer<Unmanaged_ListNode<T>>(toAdd, true);
		Count++;
	}
	public void InsertTail(T newNode)
	{
		var toAdd = Unmanaged_ListNode<T>.CreateNode<TAllocator, TAllocReturn>(newNode, _allocator);
		var prev = _tail.Pointer->Previous;
		prev.Pointer->Next = new meta_pointer<Unmanaged_ListNode<T>>(toAdd, true);
		toAdd.Pointer->Previous = prev;
		toAdd.Pointer->Next = _tail;
		_tail.Pointer->Previous = new meta_pointer<Unmanaged_ListNode<T>>(toAdd, true);
		Count++;
	}
	public void InsertFrom(meta_pointer<Unmanaged_ListNode<T>> node, T newNode)
	{
		EnsureInListOrThrow(node);
		var toAdd = Unmanaged_ListNode<T>.CreateNode<TAllocator, TAllocReturn>(newNode, _allocator);
		toAdd.Pointer->Next = node.Pointer->Next;
		toAdd.Pointer->Previous = node;
		node.Pointer->Next.Pointer->Previous = new meta_pointer<Unmanaged_ListNode<T>>(toAdd, true);
		node.Pointer->Next = new meta_pointer<Unmanaged_ListNode<T>>(toAdd, true);
		Count++;
	}
	public readonly meta_pointer<Unmanaged_ListNode<T>> FindNode(T value)
	{
		var current = _head;
		while(current != _tail)
		{
			if(EqualityComparer<T>.Default.Equals(current.Pointer->Value, value))
			{
				return current;
			}
			current = current.Pointer->Next;
		}
		return new meta_pointer<Unmanaged_ListNode<T>>();
	}
	public void RemoveNode(meta_pointer<Unmanaged_ListNode<T>> node)
	{
		EnsureInListOrThrow(node);
		node.Pointer->Previous.Pointer->Next = node.Pointer->Next;
		node.Pointer->Next.Pointer->Previous = node.Pointer->Previous;
		_allocator.Pointer->Free(node);
		Count--;
	}
	public void RemoveAllOf(T value)
	{
		var node = FindNode(value);
		while(node != NULL)
		{
			RemoveNode(node);
			node = FindNode(value);
		}
	}
	public void RemoveValue(T value)
	{
		var node = FindNode(value);
		if(node != NULL)
		{
			RemoveNode(node);
		}
	}

	public void Dispose()
	{
		Count = 0;
		while(_head != NULL)
		{
			var curr = _head;
			_head = _head.Pointer->Next;
			_allocator.Pointer->Free(curr);
		}
		_head = new meta_pointer<Unmanaged_ListNode<T>>();
		_tail = new meta_pointer<Unmanaged_ListNode<T>>();
	}

	public readonly T this[int idx]
	{
		get
		{
			if(idx < 0 || idx >= Count)
			{
				throw new IndexOutOfRangeException();
			}
			var current = _head;
			for(int i = 0; i < idx; i++)
			{
				current = current.Pointer->Next;
			}
			return current.Pointer->Value;
		}
		set
		{
			if(idx < 0 || idx >= Count)
			{
				throw new IndexOutOfRangeException();
			}
			var current = _head;
			for(int i = 0; i < idx; i++)
			{
				current = current.Pointer->Next;
			}
			current.Pointer->Value = value;
		}
	}

	private readonly void EnsureInListOrThrow(meta_pointer<Unmanaged_ListNode<T>> node)
	{
		var current = _head;
		while(current != _tail)
		{
			if(current == node)
			{
				return;
			}
			current = current.Pointer->Next;
		}
		throw new InvalidOperationException("The provided node is not part of this list.");
	}

	public readonly IEnumerator<T> GetEnumerator() => new Unmanaged_LinkedListEnumerator<T, TAllocator, TAllocReturn>(this);

	unsafe readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
internal unsafe record struct Unmanaged_ListNode<T> 
	where T : unmanaged
{
	public T Value { get; set; }
	public meta_pointer<Unmanaged_ListNode<T>> Next { get; set; }
	public meta_pointer<Unmanaged_ListNode<T>> Previous { get; set; }

	public static meta_pointer<Unmanaged_ListNode<T>> CreateEmpty<TAllocator, TAllocReturn>(meta_pointer<TAllocator> allocator)
		where TAllocator : unmanaged, IAllocator<TAllocReturn>
		where TAllocReturn : IAllocReturn => CreateNode<TAllocator, TAllocReturn>(default, allocator);
	public static meta_pointer<Unmanaged_ListNode<T>> CreateNode<TAllocator, TAllocReturn>(T value, meta_pointer<TAllocator> allocator)
		where TAllocator : unmanaged, IAllocator<TAllocReturn>
		where TAllocReturn : IAllocReturn
	{
		var buffer = allocator.Pointer->Allocate<Unmanaged_ListNode<T>>(1).Pointer.As<Unmanaged_ListNode<T>>();
		if(buffer.Pointer == NULL)
		{
			throw new OutOfMemoryException();
		}
		*buffer.Pointer = new Unmanaged_ListNode<T>
		{
			Value = value,
			Next = new meta_pointer<Unmanaged_ListNode<T>>(),
			Previous = new meta_pointer<Unmanaged_ListNode<T>>()
		};
		return buffer;
	}
}
