using CSPP.lib.std.ptr;

namespace CSPP.lib.std.allocator;

internal interface IAllocator<TAllocResult> where TAllocResult : IAllocReturn
{
	public TAllocResult Allocate<T>(int length) where T : unmanaged;
	public virtual void Free<T>(meta_pointer<T> ptr) where T : unmanaged => Free((meta_pointer)ptr);
	public virtual void Free(meta_pointer ptr)
	{
		if(!ptr.IsNative)
		{
			return;
		}
		free(ptr.Pointer);
	}
}
