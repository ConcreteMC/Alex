using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class RedstoneWire : Block
	{
		public RedstoneWire() : base(2822)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
