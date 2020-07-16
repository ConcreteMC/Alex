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
	    public Slab(uint blockStateId) : base(blockStateId)
	    {
		    Solid = true;
		    Transparent = true;
		    IsFullCube = false;
	    }
    }
}
