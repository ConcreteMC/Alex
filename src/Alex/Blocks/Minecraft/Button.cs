namespace Alex.Blocks.Minecraft
{
    public class Button : Block
    {
	    public Button(uint id) : base()
	    {
		    Transparent = true;
		    Solid = false;
		    IsFullCube = false;
		    CanInteract = true;
		    
		    BlockMaterial = Material.Circuits;
		    Hardness = 0.5f;
		    /*
		     * Solid = false;
		    Transparent = true;
		    IsReplacible = false;
		     */
	    }
	}
}
