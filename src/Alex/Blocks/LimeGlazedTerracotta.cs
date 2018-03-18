using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class LimeGlazedTerracotta : Block
	{
		public LimeGlazedTerracotta() : base(8243)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
