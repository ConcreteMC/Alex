using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class LightGrayGlazedTerracotta : Block
	{
		public LightGrayGlazedTerracotta() : base(8255)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = false;
		}
	}
}
