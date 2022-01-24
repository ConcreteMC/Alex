using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft.Stairs
{
	public class SandstoneStairs : Stairs
	{
		public SandstoneStairs() : base(4571)
		{
			Solid = true;
			Transparent = true;

			base.BlockMaterial = Material.Stone.Clone().WithMapColor(MapColor.Sand);
			;
		}
	}
}