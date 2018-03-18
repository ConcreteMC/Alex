using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Cactus : Block
	{
		public Cactus() : base(3335)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;

			BlockMaterial = Material.Cactus;
		}
	}
}
