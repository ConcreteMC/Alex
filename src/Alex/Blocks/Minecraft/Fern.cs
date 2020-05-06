using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
    public class Fern : Block
    {
	    public Fern() : base(952)
	    {
		    Solid = false;
		    Transparent = true;

		    IsFullCube = false;
		    BlockMaterial = Material.Plants;
	    }
    }
}
