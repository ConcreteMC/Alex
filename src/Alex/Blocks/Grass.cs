using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Grass : Block
	{
		public Grass() : base(951)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;

			BlockMaterial = Material.Grass;
		}
	}
}
