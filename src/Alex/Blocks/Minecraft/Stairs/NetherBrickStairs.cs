using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft.Stairs
{
	public class NetherBrickStairs : Stairs
	{
		public NetherBrickStairs() : base(4449)
		{
			Solid = true;
			Transparent = true;

			base.BlockMaterial = Material.Wood.Clone().WithMapColor(MapColor.Nether);
		}
	}
}