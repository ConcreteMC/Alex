using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class WhiteGlazedTerracotta : Block
	{
		public WhiteGlazedTerracotta() : base(8223)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
