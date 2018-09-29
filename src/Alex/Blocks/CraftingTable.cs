using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class CraftingTable : Block
	{
		public CraftingTable() : base(2960)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
