using Alex.API.Utils;
using Alex.API.World;

namespace Alex.Blocks.Minecraft
{
	public class Rail : Block
	{
		public Rail() : base(3089)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;

			RequiresUpdate = true;
		}

		public override void BlockUpdate(IWorld world, BlockCoordinates position, BlockCoordinates updatedBlock)
		{
			base.BlockUpdate(world, position, updatedBlock);
		}
	}
}
