using Alex.API.Blocks.State;
using Alex.API.World;
using Alex.Blocks.State;

namespace Alex.Items
{
    public class ItemBlock : Item
    {
	    public BlockState Block { get; }
	    public ItemBlock(BlockState block)
	    {
		    this.Block = block;
		    Name = block.Name;
		    DisplayName = block.Block.DisplayName;
	    }

	    public override Item Clone()
	    {
		    return new ItemBlock(Block)
		    {
			    Count = Count,
			    Id = Id,
			    Material = Material,
			    Meta = Meta,
			    Name = Name,
			    Nbt = Nbt,
			    Renderer = Renderer,
			    DisplayName = DisplayName,
			    ItemType = ItemType,
			    MaxStackSize = MaxStackSize
		    };
	    }
    }
}
