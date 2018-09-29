using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class RedstoneOre : Block
	{
		public RedstoneOre() : base(3290)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
