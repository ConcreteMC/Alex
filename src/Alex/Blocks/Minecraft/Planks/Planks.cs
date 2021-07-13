using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Planks
{
    public class Planks : Block
    {
        public Planks()
        {
            Solid = true;
            IsFullCube = true;
            
            BlockMaterial = Material.Wood;
        }
    }
}