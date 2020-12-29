using Alex.API.Blocks;
using Alex.ResourcePackLib.Json;

namespace Alex.Blocks.Minecraft
{
	public class WoodenSlab : Slab
	{
		protected WoodenSlab(uint blockStateId) : base(blockStateId)
		{
			BlockMaterial = Material.Wood;
		}
	}
	
    public class Slab : Block
    {
	    public Slab(uint blockStateId) : base()
	    {
		    Solid = true;
		    Transparent = true;
	    }

	    /// <inheritdoc />
	    public override bool IsFullCube {
		    get
		    {
			    if (BlockState.TryGetValue("type", out string t))
			    {
				    if (t == "double")
					    return true;
			    }
			    return false;
		    }
		    set { }
	    }
    }
}
