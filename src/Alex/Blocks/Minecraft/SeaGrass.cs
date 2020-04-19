namespace Alex.Blocks.Minecraft
{
    public class SeaGrass : Block
    {
        public SeaGrass()
        {
            Transparent = true;
            Solid = false;

            Animated = true;
            IsFullCube = false;
            
            BlockMaterial = Material.Coral;
        }
    }
}