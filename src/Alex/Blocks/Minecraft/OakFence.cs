using Alex.API.Blocks.State;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.State;

namespace Alex.Blocks.Minecraft
{
	
	public class MultiStateBlock : Block
	{
		public MultiStateBlock()
		{
			RequiresUpdate = true;
		}
		
		public override void BlockUpdate(IWorld world, BlockCoordinates position, BlockCoordinates updatedBlock)
		{
			if (BlockState is BlockState state)
			{
				if (state.MultiPartHelper != null)
				{
					
				}
			}
			base.BlockUpdate(world, position, updatedBlock);
		}

		public override IBlockState BlockPlaced(IWorld world, IBlockState state, BlockCoordinates position)
		{
			return base.BlockPlaced(world, state, position);
		}
	}
	
	public class Fence : Block
	{
		public Fence(uint blockStateId) : base()
		{
			Transparent = true;
			Solid = true;

		}

		public Fence(string name) : base()
		{
			Transparent = true;
			Solid = true;

		}

		public Fence()
		{
			Transparent = true;
			Solid = true;

		}
	}

	public class OakFence : Fence
    {
	    public OakFence() : base(3401)
	    {
	    }
    }

	public class SpruceFence : Fence
	{
		public SpruceFence() : base("minecraft:spruce_fence")
		{
		}
	}
}
