using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Farmland : Block
	{
		public Farmland() : base()
		{
			Solid = true;
			Transparent = true;

			base.BlockMaterial = Material.Dirt;
		}
	}
}