using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class EndStoneBricks : Block
	{
		public EndStoneBricks() : base(8067)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			LightOpacity = 255;
		}
	}
}
