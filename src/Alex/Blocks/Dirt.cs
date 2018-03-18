using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Dirt : Block
	{
		public Dirt() : base(10)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			BlockMaterial = Material.Ground;
		}
	}
}
