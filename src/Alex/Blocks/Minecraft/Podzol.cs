using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class Podzol : Block
	{
		public Podzol() : base()
		{
			Solid = true;
			Transparent = false;

			base.BlockMaterial = Material.Dirt.Clone().WithMapColor(MapColor.Podzol);
		}
	}
}