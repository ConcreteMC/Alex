using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft.Stairs
{
	public class BirchStairs : WoodStairs
	{
		public BirchStairs() : base()
		{
			Solid = true;
			Transparent = true;
			
			base.BlockMaterial = Material.Wood.Clone().WithMapColor(MapColor.Sand);
		}
	}
}
