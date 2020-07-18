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

		    BlockMaterial = Material.Coral;
		    //  BlockMaterial = Material.Water;
	    }
    }
}
