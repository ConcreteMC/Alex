using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Sand : Block
	{
		public Sand() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Sand;
		}
	}
}