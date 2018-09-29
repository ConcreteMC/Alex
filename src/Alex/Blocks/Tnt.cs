using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Tnt : Block
	{
		public Tnt() : base(1036)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			BlockMaterial = Material.Tnt;
		}
	}
}
