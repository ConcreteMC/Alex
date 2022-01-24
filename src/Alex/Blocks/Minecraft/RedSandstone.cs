using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class RedSandstone : Block
	{
		public RedSandstone() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Stone;
		}
	}
}