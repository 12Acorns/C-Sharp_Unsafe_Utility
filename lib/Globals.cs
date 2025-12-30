global using static CSPP.lib.std.std_usage;
global using static CSPP.lib.Globals;

namespace CSPP.lib;

internal static unsafe class Globals
{
	public static readonly int PTR_SIZE = sizeof(void*);
	public static readonly void* NULL = (void*)0;
	public const nint NULLPTR = 0;
}
