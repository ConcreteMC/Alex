using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class GreenGlazedTerracotta : Block
	{
		public GreenGlazedTerracotta() : base(8275)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
