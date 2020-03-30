using Alex.API.Utils;
using Alex.API.World;

namespace Alex.Blocks.Minecraft
{
	public class Fence : Block
	{
		public Fence(uint blockStateId) : base(blockStateId)
		{
			Transparent = true;
			Solid = true;
			
			
		}

		public Fence(string name) : base(name)
		{
			Transparent = true;
			Solid = true;
			
			
		}

		public Fence()
		{
			Transparent = true;
			Solid = true;
			
			
		}

		public override void BlockUpdate(IWorld world, BlockCoordinates position, BlockCoordinates updatedBlock)
		{
			base.BlockUpdate(world, position, updatedBlock);
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
