using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class LightBlueGlazedTerracotta : Block
	{
		public LightBlueGlazedTerracotta() : base(8235)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
