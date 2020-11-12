namespace Alex.Blocks.Minecraft
{
    public class Peony : FlowerBase
    {
        public Peony()
        {
            Solid = false;
            Transparent = true;

            IsFullBlock = false;
            IsFullCube = false;

            BlockMaterial = Material.Plants;
        }      
    }
}