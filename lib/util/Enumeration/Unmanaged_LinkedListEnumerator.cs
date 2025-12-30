using CSPP.lib.std.STL.STL_98.List;
using CSPP.lib.std.allocator;
using System.Collections;

namespace CSPP.lib.util.Enumeration;

internal sealed unsafe class Unmanaged_LinkedListEnumerator<T, TAllocator, TAllocReturn> : IEnumerator<T>
	where T : unmanaged
	where TAllocator : unmanaged, IAllocator<TAllocReturn>
	where TAllocReturn : IAllocReturn
{
	private readonly Unmanaged_List<T, TAllocator, TAllocReturn> _list;

	private Unmanaged_ListNode<T> _currentNode;
	private int _position = 0;

	public Unmanaged_LinkedListEnumerator(Unmanaged_List<T, TAllocator, TAllocReturn> list)
	{
		_list = list;
		_currentNode = *_list._head.Pointer;
	}

	public T Current => _currentNode.Value;
	object IEnumerator.Current => Current;

	public IEnumerator<T> GetEnumerator()
	{
		if(!MoveNext())
		{
			yield break;
		}
		yield return _currentNode.Value;
	}
	public unsafe bool MoveNext()
	{
		if(_currentNode.Next.Pointer == NULL || *_currentNode.Next.Pointer == default ||
		   _currentNode.Next.Pointer == _list._tail.Pointer)
		{
			return false;
		}
		_currentNode = *_currentNode.Next.Pointer;
		_position++;
		return true;
	}
	public void Reset()
	{
		_currentNode = *_list._head.Pointer;
		_position = 0;
	}
	public void Dispose()
	{
	}
}
