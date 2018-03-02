using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Blocks.State;
using Alex.API.World;

namespace Alex.Blocks.State
{
    public class StateImplementation : BlockStateBase
    {
		private IBlock Block { get; }
	    public StateImplementation(IBlock block)
	    {
		    Block = block;
	    }

	    public override IBlock GetBlock()
	    {
		    return Block;
	    }
    }
}
