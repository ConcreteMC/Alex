using Alex.Blocks.Materials;
using Alex.Entities.BlockEntities;

namespace Alex.Blocks.Minecraft;

public class WallBanner : Block
{
    public WallBanner(BlockColor color)
    {
        Solid = false;
        Transparent = true;
        Renderable = false;
        CanInteract = true;
			
        HasHitbox = true;
        
        BlockMaterial = Material.Wool.Clone().WithMapColor(color.ToMapColor());
    }
}

public class StandingBanner : Block
{
    public StandingBanner(BlockColor color)
    {
        Solid = false;
        Transparent = true;
        Renderable = false;
        CanInteract = true;
			
        HasHitbox = true;
        
        BlockMaterial = Material.Wool.Clone().WithMapColor(color.ToMapColor());
    }
}