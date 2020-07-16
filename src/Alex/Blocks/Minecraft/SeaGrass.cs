using Alex.API.Utils;
using Alex.Blocks.State;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;
using Alex.Worlds.Multiplayer.Java;

namespace Alex.Blocks.Minecraft
{
    public class SeaGrass : Block
    {
        public SeaGrass()
        {
            IsWater = true;
            Transparent = true;
            Solid = false;

            Animated = true;
            IsFullCube = false;

            BlockMaterial = Material.Coral;
        }
    }
}