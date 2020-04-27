namespace Alex.Blocks.Minecraft
{
    public class Grindstone : Block
    {
        public Grindstone()
        {
            Transparent = true;
            Solid = true;
            IsFullBlock = false;
            IsFullCube = false;
            
            BlockMaterial = Material.Stone;
        }
    }
}