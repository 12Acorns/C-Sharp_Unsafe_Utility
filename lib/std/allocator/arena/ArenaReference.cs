using CSPP.lib.std.ptr;

namespace CSPP.lib.std.allocator.arena;

internal readonly unsafe record struct ArenaReference : IAllocReturn
{
	public ArenaReference(nint start, nint offset, int length, int stride) =>
		(Pointer, Start, Offset, Length, LengthInBytes) = (new(offset), new(start, true), offset, length, length * stride);

	public meta_pointer Pointer { get; internal init; }
	public meta_pointer Start { get; internal init; }
	public nint Offset { get; internal init; }
	public int Length { get; internal init; }
	public int LengthInBytes { get; internal init; }

	public static bool operator ==(ArenaReference left, nint right) => left.Pointer == right;
	public static bool operator !=(ArenaReference left, nint right) => left.Pointer != right;

	public static bool operator ==(ArenaReference left, void* right) => left.Pointer == (nint)right;
	public static bool operator !=(ArenaReference left, void* right) => left.Pointer != (nint)right;
}

internal readonly unsafe record struct ArenaReference<T> : IAllocReturn
	where T : unmanaged
{
	public ArenaReference(T* start, nint offset, int length) =>
		(Pointer, Offset, Length) = (new meta_pointer<T>(start, true), offset, length);
	public ArenaReference(nint start, nint offset, int length) =>
		(Pointer, Start, Offset, Length) = (new((T*)offset), new((T*)start, true), offset, length);

	public meta_pointer<T> Pointer { get; internal init; }
	public meta_pointer<T> Start { get; internal init; }
	public nint Offset { get; internal init; }
	public int Length { get; internal init; }
	public int LengthInBytes => Length * sizeof(T);

	meta_pointer IAllocReturn.Pointer { get; }

	public static bool operator ==(ArenaReference<T> left, nint right) => left.Pointer == right;
	public static bool operator !=(ArenaReference<T> left, nint right) => left.Pointer != right;

	public static bool operator ==(ArenaReference<T> left, void* right) => left.Pointer == (nint)right;
	public static bool operator !=(ArenaReference<T> left, void* right) => left.Pointer != (nint)right;

	public static implicit operator ArenaReference(ArenaReference<T> reference) =>
		new ArenaReference((nint)reference.Pointer.Pointer, reference.Offset, reference.Length, reference.LengthInBytes);
	public static explicit operator ArenaReference<T>(ArenaReference reference)
	{
		if(reference.LengthInBytes % sizeof(T) != 0)
		{
			throw new InvalidCastException("Cannot convert ArenaReference to ArenaReference<T> with different length.");
		}
		return new ArenaReference<T>((T*)reference.Pointer.Pointer, reference.Offset, reference.Length);
	}
}
