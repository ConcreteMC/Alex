using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class ChainCommandBlock : Block
	{
		public ChainCommandBlock() : base(8092)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			LightOpacity = 16;
		}
	}
}
