using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft.Slabs
{
	public class SandstoneSlab : Slab
	{
		public SandstoneSlab() : base()
		{
			base.BlockMaterial = Material.Stone.Clone().WithMapColor(MapColor.Sand);
			;
		}
	}
}