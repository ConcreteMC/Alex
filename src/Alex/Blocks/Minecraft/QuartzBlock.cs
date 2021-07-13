using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class QuartzBlock : Block
	{
		public QuartzBlock() : base()
		{
			Solid = true;
			Transparent = false;
			
			base.BlockMaterial = Material.Stone.Clone().WithMapColor(MapColor.Quartz);
		}
	}
}
