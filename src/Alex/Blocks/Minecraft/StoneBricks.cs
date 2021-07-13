using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class StoneBricks : Block
	{
		public StoneBricks() : base()
		{
			Solid = true;
			Transparent = false;
			IsFullCube = true;
			
			BlockMaterial = Material.Stone.Clone().WithHardness(1.5f);
		}
	}
}
