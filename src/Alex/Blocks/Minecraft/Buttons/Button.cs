using Alex.Blocks.State;

namespace Alex.Blocks.Minecraft
{
    public class Button : RedstoneBase
    {
	    public Button(uint id) : base()
	    {
		    Transparent = true;
		    Solid = false;
		    IsFullCube = false;
		    CanInteract = true;
		    
		    BlockMaterial = Material.Circuits;
		    /*
		     * Solid = false;
		    Transparent = true;
		    IsReplacible = false;
		     */
	    }

	    /// <inheritdoc />
	    public override bool TryGetStateProperty(string prop, out StateProperty stateProperty)
	    {
		    return base.TryGetStateProperty(prop, out stateProperty);
	    }
    }
}
