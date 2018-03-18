using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class RedstoneBlock : Block
	{
		public RedstoneBlock() : base(5593)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
