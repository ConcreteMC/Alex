using System;
using System.Collections.Generic;
using System.Text;
using Alex.Blocks;

namespace Alex.Items
{
    public class ItemBlock : Item
    {
	    public Block Block { get; }
	    public ItemBlock(Block block)
	    {
		    this.Block = block;
	    }
    }
}
