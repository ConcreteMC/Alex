using Alex.API.Blocks;
using Alex.API.World;

namespace Alex.Blocks.Minecraft
{
	public class GlassPane : Block
	{
		public GlassPane() : base()
		{
			Solid = true;
			Transparent = true;
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
