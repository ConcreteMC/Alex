using Alex.API.Blocks;
using Alex.API.World;

namespace Alex.Blocks.Minecraft
{
	public class GlassPane : Block
	{
		public GlassPane() : base(4152)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullCube = false;

			BlockMaterial = Material.Glass;
		}
		
		public override bool CanAttach(BlockFace face, Block block)
		{
			if (block is GlassPane)
				return true;
			
			return base.CanAttach(face, block);
		}
	}
}
