using CSPP.lib.std.allocator.arena;
using CSPP.lib.std.STL.STL_98.List;
using CSPP.lib.std.allocator;
using CSPP.lib.std.text;
using CSPP.lib.std.ptr;

unsafe
{
	main();

	int main()
	{
		using var arena = ArenaAllocator.CreateUnmanaged<char>(30, AllocatorDisposalMode.ClearWhole);

		var c_str = new c_string<ArenaAllocator, ArenaReference>(arena, "Hello, World!");

		print_str(c_str);

		using var listArena = ArenaAllocator.CreateUnmanaged<byte>(
			sizeof(Unmanaged_List<int, ArenaAllocator, ArenaReference>) + 300 * sizeof(Unmanaged_ListNode<int>),
			AllocatorDisposalMode.ClearWhole);
		var listPtr = Unmanaged_List<int, ArenaAllocator, ArenaReference>.CreateUnmanaged(listArena);
		
		listPtr.Pointer->InsertStart(10);
		listPtr.Pointer->InsertSort(5);
		listPtr.Pointer->InsertSort(2);
		listPtr.Pointer->InsertSort(8);
		listPtr.Pointer->InsertStart(22);
		listPtr.Pointer->InsertTail(1);

		print_lst(listPtr);

		var list = *listPtr.Pointer;

		print_enumerator(list.Select(x => x * 2));

		return 1;
	}
	void print_str<TAlloc, TAllocReturn>(c_string<TAlloc, TAllocReturn> str) 
		where TAlloc : unmanaged, IAllocator<TAllocReturn>
		where TAllocReturn : unmanaged, IAllocReturn
	{
		Console.WriteLine($"Content: {str}, Length: {str.Length()}");
	}
	void print_lst<T, TAllocator, TAllocReturn>(meta_pointer<Unmanaged_List<T, TAllocator, TAllocReturn>> list) 
		where T : unmanaged
		where TAllocator : unmanaged, IAllocator<TAllocReturn>
		where TAllocReturn : IAllocReturn
	{
		Console.Write("List content: ");
		var current = list.Pointer->Start;
		while(current != list.Pointer->End)
		{
			Console.Write($"{current.Pointer->Value}->");
			current = current.Pointer->Next;
		}
		Console.CursorLeft -= 2;
		Console.Write($"   | len: {list.Pointer->Count}");
		Console.WriteLine();
	}
	void print_enumerator<T>(IEnumerable<T> enumerator)
		where T : unmanaged
	{
		Console.Write("List content: ");
		foreach(var item in enumerator)
		{
			Console.Write($"{item}->");
		}
		Console.CursorLeft -= 2;
		Console.Write($"   | len: {enumerator.Count()}");
		Console.WriteLine();
	}
}