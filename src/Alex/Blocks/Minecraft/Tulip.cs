namespace Alex.Blocks.Minecraft
{
    public class Tulip : Block
    {
        public Tulip()
        {
            Solid = false;
            Transparent = true;

            IsFullBlock = false;
            IsFullCube = false;

            BlockMaterial = Material.Plants;
        }
    }
}