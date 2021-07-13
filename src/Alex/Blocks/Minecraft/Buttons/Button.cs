using Alex.Blocks.Materials;
using Alex.Blocks.State;

namespace Alex.Blocks.Minecraft.Buttons
{
    public class Button : RedstoneBase
    {
	    public Button(uint id) : base()
	    {
		    Transparent = true;
		    Solid = false;
		    IsFullCube = false;
		    CanInteract = true;
		    
		    BlockMaterial = Material.Circuits;
		    /*
		     * Solid = false;
		    Transparent = true;
		    IsReplacible = false;
		     */
	    }
    }
}
