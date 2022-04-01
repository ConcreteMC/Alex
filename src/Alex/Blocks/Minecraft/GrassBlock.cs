using Alex.Blocks.Materials;
using Alex.Blocks.Properties;

namespace Alex.Blocks.Minecraft
{
	public class GrassBlock : Block
	{
		private static readonly PropertyBool Snowy = new PropertyBool("snowy", "true", "false");

		public GrassBlock() : base()
		{
			Solid = true;
			Transparent = false;

			base.BlockMaterial = Material.Grass;
		}
	}
}