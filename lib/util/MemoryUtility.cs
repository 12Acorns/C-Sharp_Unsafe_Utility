using System.Runtime.CompilerServices;

namespace CSPP.lib.util;

internal static unsafe class MemoryUtility
{
	/// <returns>Returns a pointer to a struct allocated on unmanaged memory</returns>
	/// <exception cref="OutOfMemoryException"></exception>
	public static T* AllocStructureUnmanaged<T>() where T : unmanaged
	{
		var size = sizeof(T);
		var ptr = (T*)malloc(size);
		if (ptr == null)
		{
			throw new OutOfMemoryException($"Failed to allocate memory for structure of type {typeof(T).Name}.");
		}
		return ptr;
	}
	/// <exception cref="ArgumentException"></exception>
	public static ref T GetStructureFromSpan<T>(Span<byte> allocBuffer) where T : unmanaged
	{
		var size = sizeof(T);
		if(allocBuffer.Length != size)
		{
			throw new ArgumentException($"Invalid allocation buffer size: {allocBuffer.Length}. Expected: {size}.");
		}
		return ref Unsafe.As<byte, T>(ref allocBuffer.GetPinnableReference());
	}
}
