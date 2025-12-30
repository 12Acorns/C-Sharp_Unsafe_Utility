using System.Runtime.CompilerServices;
using CSPP.lib.std.ptr;
using CSPP.lib.util;

namespace CSPP.lib.std.allocator.dynamic;

/// <param name="Size"> In bytes </param>
/// <param name="Pointer"> Data </param>
public unsafe readonly record struct ListNode(int Size, meta_pointer Pointer) : IAllocReturn
{
	public static void Init(ListNode* dst, ListNode* data)
	{
		if(dst == null)
		{
			throw new ArgumentNullException(nameof(dst), "Destination node pointer cannot be null.");
		}
		*dst = new ListNode(data->Size, data->Pointer);
	}
	public static void Init(ListNode* dst, int size, meta_pointer data)
	{
		if(dst == null)
		{
			throw new ArgumentNullException(nameof(dst), "Destination node pointer cannot be null.");
		}
		*dst = new ListNode(size, data);
	}
	public static ref ListNode Create(Span<byte> allocBuffer, int size, meta_pointer data)
	{
		if(allocBuffer.Length != sizeof(ListNode))
		{
			throw new ArgumentException($"Invalid allocation buffer size: {allocBuffer.Length}. Expected: {sizeof(ListNode)}.");
		}

		ref var node = ref MemoryUtility.GetStructureFromSpan<ListNode>(allocBuffer);
		node = new ListNode(size, data);
		return ref node;
	}
	public static ListNode* Create(int size, meta_pointer data) => (ListNode*)Unsafe.AsPointer(
		ref Create(new Span<byte>((void*)malloc(sizeof(ListNode)), sizeof(ListNode)), size, data));
}
