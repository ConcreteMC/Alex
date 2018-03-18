using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Cake : Block
	{
		public Cake() : base(3416)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;

			BlockMaterial = Material.Cake;
		}
	}
}
