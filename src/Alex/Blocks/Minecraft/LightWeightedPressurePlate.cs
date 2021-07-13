using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class LightWeightedPressurePlate : Block
	{
		public LightWeightedPressurePlate() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial  = Material.Iron.Clone().WithHardness(0.5f);
		}
	}
}
