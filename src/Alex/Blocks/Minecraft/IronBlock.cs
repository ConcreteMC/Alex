using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class IronBlock : Block
	{
		public IronBlock() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Iron;
		}
	}
}
