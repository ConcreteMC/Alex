using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Cauldron : Block
	{
		public Cauldron() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Iron;
		}
	}
}
