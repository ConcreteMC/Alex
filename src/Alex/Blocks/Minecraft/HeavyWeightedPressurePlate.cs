using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class HeavyWeightedPressurePlate : PressurePlate
	{
		public HeavyWeightedPressurePlate() : base()
		{
			Solid = true;
			Transparent = true;

			//Hardness = 0.5f;
			
			BlockMaterial  = Material.Metal.Clone().WithHardness(0.5f);
		}
	}
}
