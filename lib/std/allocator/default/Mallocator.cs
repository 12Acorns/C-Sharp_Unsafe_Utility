namespace CSPP.lib.std.allocator.@default;

internal unsafe readonly struct Mallocator : IAllocator<MallocatorReturn>
{
	public static readonly Mallocator Instance = new Mallocator();

	/// <returns>
	/// <see cref="MallocatorReturn"/>
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	/// <exception cref="OutOfMemoryException"></exception>
	public MallocatorReturn Allocate<T>(int length) where T : unmanaged
	{
		if(length < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative.");
		}
		var ptr = malloc(checked(length * sizeof(T)));
		if(ptr == NULLPTR)
		{
			throw new OutOfMemoryException("Memory allocation failed.");
		}
		return new MallocatorReturn(new((void*)ptr, true));
	}
}
