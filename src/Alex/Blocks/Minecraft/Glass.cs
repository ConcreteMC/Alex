using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Glass : Block
	{
		public Glass() : base()
		{
			Solid = true;
			Transparent = true;
			base.IsFullCube = true;

			base.BlockMaterial = Material.Glass;
		}
	}

	public class StainedGlass : Glass
	{
		public StainedGlass()
		{
			Solid = true;
			Transparent = true;
			base.IsFullCube = true;

			base.BlockMaterial = Material.Glass;
		}
	}
}
