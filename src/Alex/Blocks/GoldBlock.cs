using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class GoldBlock : Block
	{
		public GoldBlock() : base(1033)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
