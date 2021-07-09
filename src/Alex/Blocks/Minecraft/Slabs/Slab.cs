using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Common.Blocks.Properties;

namespace Alex.Blocks.Minecraft.Slabs
{
	public class WoodenSlab : Slab
	{
		protected WoodenSlab() : base()
		{
			BlockMaterial = Material.Wood;
		}
	}
	
    public class Slab : Block
    {
	    public Slab() : base()
	    {
		    Solid = true;
		    Transparent = true;
	    }

	    /// <inheritdoc />
	    public override bool IsFullCube {
		    get
		    {
			    if (BlockState.TryGetValue("type", out string t))
			    {
				    if (t == "double")
					    return true;
			    }
			    return false;
		    }
		    set { }
	    }

	    private static readonly PropertyString TypeProp = new PropertyString("type");

	    /// <inheritdoc />
	    public override bool TryGetStateProperty(string prop, out IStateProperty stateProperty)
	    {
		    switch (prop)
		    {
			    case "type":
				    stateProperty = TypeProp;
				    return true;
		    }
		    return base.TryGetStateProperty(prop, out stateProperty);
	    }
    }
}
