using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class TallGrass : Block
	{
		public TallGrass() : base(6761)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = true;
		}
	}
}
