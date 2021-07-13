using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
    public class CornFlower : FlowerBase
    {
	    public CornFlower()
	    {
		    Transparent = true;
		    Solid = false;
		    IsFullCube = false;
		    
		    BlockMaterial = Material.Plants;
	    }
    }
}
