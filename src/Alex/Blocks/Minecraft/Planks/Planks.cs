using Alex.Blocks.Materials;
using Alex.Blocks.Minecraft.Signs;

namespace Alex.Blocks.Minecraft.Planks
{
	public class Planks : Block
	{
		public Planks(WoodType woodType = WoodType.Oak)
		{
			Solid = true;
			IsFullCube = true;

			BlockMaterial = Material.Wood.Clone().WithMapColor(woodType.ToMapColor());
		}
	}
}