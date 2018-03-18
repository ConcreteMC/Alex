using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Piston : Block
	{
		public Piston() : base(963)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			BlockMaterial = Material.Piston;
		}
	}
}
