namespace Alex.Blocks.Minecraft
{
    public class Leaves : Block
    {
	    public Leaves(uint blockStateId) : base(blockStateId)
	    {
		    Solid = true;
		    Transparent = true;
		    IsReplacible = false;
		    IsFullBlock = true;
		    IsFullCube = true;

		    LightOpacity = 2;
		    
		    BlockMaterial = Material.Leaves;
		    Hardness = 0.2f;
	    }
    }
}
