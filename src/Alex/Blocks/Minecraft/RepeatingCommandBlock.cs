using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class RepeatingCommandBlock : Block
	{
		public RepeatingCommandBlock() : base()
		{
			Solid = true;
			Transparent = false;
			IsFullCube = true;

			base.BlockMaterial = Material.Metal;
		}
	}
}
