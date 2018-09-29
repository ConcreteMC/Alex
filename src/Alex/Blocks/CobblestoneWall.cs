using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class CobblestoneWall : Block
	{
		public CobblestoneWall() : base(5110)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
