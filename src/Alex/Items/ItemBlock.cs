using Alex.API.Blocks.State;

namespace Alex.Items
{
    public class ItemBlock : Item
    {
	    public IBlockState Block { get; }
	    public ItemBlock(IBlockState block)
	    {
		    this.Block = block;
	    }
    }
}
