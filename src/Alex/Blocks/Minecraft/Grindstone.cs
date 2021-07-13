using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
    public class Grindstone : Block
    {
        public Grindstone()
        {
            Transparent = true;
            Solid = true;
            IsFullCube = false;
            
            BlockMaterial = Material.Anvil;
        }
    }
}