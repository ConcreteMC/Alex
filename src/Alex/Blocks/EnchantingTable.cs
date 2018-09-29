using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class EnchantingTable : Block
	{
		public EnchantingTable() : base(4522)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
