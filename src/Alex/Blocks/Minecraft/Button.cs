namespace Alex.Blocks.Minecraft
{
    public class Button : Block
    {
	    public Button(uint id) : base(id)
	    {
		    Transparent = true;
		    Solid = true;
		    IsFullCube = false;

			/*
			 * Solid = false;
			Transparent = true;
			IsReplacible = false;
			 */
		}
	}
}
