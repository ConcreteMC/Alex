using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Sand : Block
	{
		public Sand() : base(66)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			BlockMaterial = Material.Sand;
		}
	}
}
