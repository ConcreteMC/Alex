namespace Alex.Blocks.Minecraft
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

		    Animated = true;
		    
		    BlockMaterial = Material.WaterPlant;
		    //  BlockMaterial = Material.Water;
	    }
    }
}
