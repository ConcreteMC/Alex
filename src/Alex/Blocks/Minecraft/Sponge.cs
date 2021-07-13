using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Sponge : Block
	{
		public Sponge() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Sponge;
		}
	}
}
