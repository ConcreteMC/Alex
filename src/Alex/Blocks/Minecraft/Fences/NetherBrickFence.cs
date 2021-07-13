using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft.Fences
{
	public class NetherBrickFence : Fence
	{
		public NetherBrickFence() : base()
		{
			base.BlockMaterial = Material.Wood.Clone().WithMapColor(MapColor.Nether);
		}
	}
}
