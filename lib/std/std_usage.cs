using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CSPP.lib.std;

internal static unsafe class std_usage
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static nint realloc(nint ptr, nint size) =>
		(nint)NativeMemory.Realloc((void*)ptr, checked((nuint)size));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static nint realloc(nint ptr, int size) =>
		(nint)NativeMemory.Realloc((void*)ptr, checked((nuint)size));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static nint malloc(nint sizeInBytes) =>
		(nint)NativeMemory.Alloc(checked((nuint)sizeInBytes));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static nint malloc(int sizeInBytes) =>
		(nint)NativeMemory.Alloc(checked((nuint)sizeInBytes));
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void free(nint ptr) => NativeMemory.Free((void*)ptr);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void free(void* ptr) => NativeMemory.Free(ptr);
}
