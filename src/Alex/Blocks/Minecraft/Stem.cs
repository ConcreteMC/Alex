namespace Alex.Blocks.Minecraft
{
    public class Stem : Block
    {
        public Stem()
        {
            Solid = false;
            Transparent = true;
            IsReplacible = false;
            
            BlockMaterial = Material.Plants;
        }
    }
}