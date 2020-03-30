namespace Alex.Blocks.Minecraft
{
    public class Button : Block
    {
	    public Button(uint id) : base(id)
	    {
		    Transparent = true;
		    Solid = false;
		    IsFullCube = false;

		    BlockMaterial = Material.Circuits;
		    /*
		     * Solid = false;
		    Transparent = true;
		    IsReplacible = false;
		     */
	    }
	}
}
