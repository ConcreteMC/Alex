using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class LightWeightedPressurePlate : Block
	{
		public LightWeightedPressurePlate() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Metal.Clone().WithHardness(0.5f);
		}
	}
}