using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Slabs
{
	public class StoneSlab : Slab
	{
		public StoneSlab() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Stone;
		}
	}
}
