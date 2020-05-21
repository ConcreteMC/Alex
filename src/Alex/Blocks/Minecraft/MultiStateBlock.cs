using Alex.API.Blocks.State;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.State;
using Alex.Worlds;

namespace Alex.Blocks.Minecraft
{
	public class MultiStateBlock : Block
	{
		public MultiStateBlock()
		{
			RequiresUpdate = true;
		}
		
		public override void BlockUpdate(World world, BlockCoordinates position, BlockCoordinates updatedBlock)
		{
			if (BlockState is BlockState state)
			{
				if (state.MultiPartHelper != null)
				{
					
				}
			}
			base.BlockUpdate(world, position, updatedBlock);
		}
	}
}