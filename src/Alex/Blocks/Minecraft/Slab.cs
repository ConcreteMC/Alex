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

	    public override bool IsSolid(BlockFace face)
	    {
		    switch (face)
		    {
			    case BlockFace.North:
			    case BlockFace.East:
			    case BlockFace.South:
			    case BlockFace.West:
				    return false;
				    break;
			    default:
				    return true;
		    }
	    }
    }
}
