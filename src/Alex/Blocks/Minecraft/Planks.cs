namespace Alex.Blocks.Minecraft
{
    public class Planks : Block
    {
        public Planks()
        {
            Animated = false;
            Solid = true;
            IsFullBlock = true;
            IsFullCube = true;
            
            BlockMaterial = Material.Wood;
        }
    }
}