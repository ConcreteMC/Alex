using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class SeaGrass : Block
	{
		public SeaGrass()
		{
			//IsWater = true;
			Transparent = true;
			Solid = false;

			IsFullCube = false;

			BlockMaterial = Material.ReplaceableWaterPlant;
		}
	}

	public class SeaPickle : Block
	{
		public SeaPickle()
		{
			//IsWater = true;
			Transparent = true;
			Solid = false;

			IsFullCube = false;

			BlockMaterial = Material.ReplaceableWaterPlant;
		}
	}
}