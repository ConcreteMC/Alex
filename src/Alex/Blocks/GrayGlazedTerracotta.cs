using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class GrayGlazedTerracotta : Block
	{
		public GrayGlazedTerracotta() : base(8251)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
