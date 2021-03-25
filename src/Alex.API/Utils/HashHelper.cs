using System.Runtime.CompilerServices;

namespace Alex.API.Utils
{
	internal static class HashHelper
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Combine(int h1, int h2)
		{
			unchecked
			{
				// RyuJIT optimizes this to use the ROL instruction
				// Related GitHub pull request: dotnet/coreclr#1830
				uint rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
				return ((int)rol5 + h1) ^ h2;
			}
		}
	}
}