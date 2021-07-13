using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Slabs
{
	public class StoneBrickSlab : Slab
	{
		public StoneBrickSlab() : base()
		{
			BlockMaterial = Material.Stone.Clone().WithHardness(1.5f);
			//Hardness = 1.5f;
		}
	}
}
