namespace Alex.Blocks.Minecraft.Leaves
{
    public class Leaves : Block
    {
	    public Leaves(uint blockStateId) : base()
	    {
		    Solid = true;
		    Transparent = true;
		    IsFullCube = true;

		    LightOpacity = 2;
		    
		    BlockMaterial = Material.Leaves;
	    }
    }
}
