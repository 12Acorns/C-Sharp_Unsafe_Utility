using CSPP.lib.std.ptr;

namespace CSPP.lib.std.allocator.@default;

public readonly struct MallocatorReturn : IAllocReturn, IDisposable
{
	public meta_pointer Pointer { get; }

	public MallocatorReturn(meta_pointer ptr)
	{
		Pointer = ptr;
	}

	public void Dispose()
	{
		if(Pointer.IsNative)
		{
			free(Pointer.Pointer);
		}
	}
}
