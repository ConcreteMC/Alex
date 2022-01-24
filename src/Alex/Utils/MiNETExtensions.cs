using System;
using MiNET.Utils;

namespace Alex.Utils
{
	public static class MiNETExtensions
	{
		public static UUID FromEntityId(long entityId)
		{
			var bytes = new byte[16];

			var other = BitConverter.GetBytes(entityId);

			for (var index = 0; index < other.Length; index++)
			{
				var bit = other[index];
				bytes[index] = bit;
			}

			return new MiNET.Utils.UUID(bytes);
		}

		public static string Value(this IBlockState state)
		{
			if (state is BlockStateInt bsi)
			{
				return bsi.Value.ToString();
			}

			if (state is BlockStateByte bsb)
			{
				return bsb.Value.ToString();
			}

			if (state is BlockStateString bss)
			{
				return bss.Value;
			}

			return null;
		}
	}
}