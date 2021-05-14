using Alex.API.Blocks;
using Alex.ResourcePackLib.Json;

namespace Alex.Blocks.Minecraft
{
	public class WoodenSlab : Slab
	{
		protected WoodenSlab() : base()
		{
			BlockMaterial = Material.Wood;
		}
	}
	
    public class Slab : Block
    {
	    public Slab() : base()
	    {
		    Solid = true;
		    Transparent = true;
		    IsFullBlock = false;
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
