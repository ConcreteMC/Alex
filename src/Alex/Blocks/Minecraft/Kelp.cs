namespace Alex.Blocks.Minecraft
{
	public class Kelp : Block
    {
	    public Kelp() : base()
	    {
		    IsSourceBlock = true;
		    IsWater = true;

		    Transparent = true;
		    Solid = false;
		    IsReplacible = true;

		    Animated = true;
		    
		    BlockMaterial = Material.WaterPlant;
		    //  BlockMaterial = Material.Water;
	    }
    }
}
