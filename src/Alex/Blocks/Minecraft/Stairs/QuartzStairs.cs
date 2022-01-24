using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft.Stairs
{
	public class QuartzStairs : Stairs
	{
		public QuartzStairs() : base(5621)
		{
			Solid = true;
			Transparent = true;

			base.BlockMaterial = Material.Stone.Clone().WithMapColor(MapColor.Quartz);
		}
	}
}