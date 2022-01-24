using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class SugarCane : Block
	{
		public SugarCane() : base()
		{
			Solid = false;
			Transparent = true;
			IsFullCube = false;

			BlockMaterial = Material.Plants;
		}
	}
}