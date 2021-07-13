using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft.Slabs
{
	public class QuartzSlab : Slab
	{
		public QuartzSlab() : base()
		{
			base.BlockMaterial = Material.Stone.Clone().WithMapColor(MapColor.Quartz);
		}
	}
}
