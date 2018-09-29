using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class CoalOre : Block
	{
		public CoalOre() : base(71)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
