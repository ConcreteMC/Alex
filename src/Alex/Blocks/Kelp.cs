namespace Alex.Blocks
{
	public class Kelp : Block
    {
	    public Kelp() : base("minecraft:kelp")
	    {
		    IsSourceBlock = true;
		    IsWater = true;

		    Transparent = true;
		    Solid = false;
		    IsReplacible = true;
	    }
    }
}
