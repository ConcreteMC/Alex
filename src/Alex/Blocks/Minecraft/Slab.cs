namespace Alex.Blocks.Minecraft
{
    public class Slab : Block
    {
	    public Slab(uint blockStateId) : base(blockStateId)
	    {
		    Solid = true;
		    Transparent = false;
		    IsFullCube = false;
		    LightOpacity = 15;
	    }
    }
}
