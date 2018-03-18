using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class CyanGlazedTerracotta : Block
	{
		public CyanGlazedTerracotta() : base(8259)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
