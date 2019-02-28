using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Blocks.State;
using Alex.Blocks;

namespace Alex.Items
{
    public class ItemBlock : Item
    {
	    public IBlockState Block { get; }
	    public ItemBlock(IBlockState block)
	    {
            base.isBlock = true;
		    this.Block = block;
	    }
    }
}
