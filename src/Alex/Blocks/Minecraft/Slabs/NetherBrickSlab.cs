using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft.Slabs
{
	public class NetherBrickSlab : Slab
	{
		public NetherBrickSlab() : base()
		{
			base.BlockMaterial = Material.Wood.Clone().WithMapColor(MapColor.Nether);
		}
	}
}