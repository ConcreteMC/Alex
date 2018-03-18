using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Vine : Block
	{
		public Vine() : base(4209)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;

			BlockMaterial = Material.Vine;
		}
	}
}
