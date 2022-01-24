using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Kelp : Block
	{
		public Kelp() : base()
		{
			//IsWater = true;

			Transparent = true;
			Solid = false;

			BlockMaterial = Material.WaterPlant;
			//  BlockMaterial = Material.Water;
		}
	}
}