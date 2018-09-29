using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Portal : Block
	{
		public Portal() : base(3406)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			LightValue = 11;

			BlockMaterial = Material.Portal;
		}
	}
}
