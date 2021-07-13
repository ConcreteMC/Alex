using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
    public class RoseBush : Block
    {
	    public RoseBush()
	    {
		    Solid = false;
		    Transparent = true;
		    IsFullCube = false;
		    BlockMaterial = Material.Plants;
		}
    }
}
