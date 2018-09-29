using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.Blocks
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

		public Fence() { }
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
