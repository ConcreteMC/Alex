using Alex.Blocks.Materials;
using Alex.Entities.BlockEntities;

namespace Alex.Blocks.Minecraft;

public class WallBanner : Block
{
    public BlockColor Color { get; set; }

    public WallBanner(BlockColor color)
    {
        Color = color;
        Solid = false;
        Transparent = true;
        Renderable = false;
        CanInteract = true;

        HasHitbox = true;
    }
}

public class StandingBanner : Block
{
    public BlockColor Color { get; set; }

    public StandingBanner(BlockColor color)
    {
        Color = color;

        Solid = false;
        Transparent = true;
        Renderable = false;
        CanInteract = true;

        HasHitbox = true;
    }
}