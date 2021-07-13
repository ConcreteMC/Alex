using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class WeepingVinesPlant : Block
	{
		public WeepingVinesPlant()
		{
			Transparent = true;
			Solid = false;
			
			base.BlockMaterial = Material.Plants.Clone().WithMapColor(MapColor.Nether);
		}
	}
}