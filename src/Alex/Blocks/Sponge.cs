using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Sponge : Block
	{
		public Sponge() : base(138)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			BlockMaterial = Material.Sponge;
		}
	}
}
