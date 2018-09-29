using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class BlackGlazedTerracotta : Block
	{
		public BlackGlazedTerracotta() : base(8283)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
