using Alex.Blocks.State;
using Alex.Common.Utils.Vectors;
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
			if (BlockState is BlockState state) { }

			base.BlockUpdate(world, position, updatedBlock);
		}
	}
}