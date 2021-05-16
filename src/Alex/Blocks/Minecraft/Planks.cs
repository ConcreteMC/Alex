namespace Alex.Blocks.Minecraft
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