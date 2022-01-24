using Alex.Blocks.Materials;
using Alex.Blocks.State;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;
using Alex.Worlds.Multiplayer.Java;

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