using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class MagentaGlazedTerracotta : Block
	{
		public MagentaGlazedTerracotta() : base(8231)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
