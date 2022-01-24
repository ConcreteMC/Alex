using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class CampFire : Block
	{
		public CampFire()
		{
			Luminance = 15;

			base.BlockMaterial = Material.Wood.Clone().WithMapColor(MapColor.Fire);
		}
	}
}