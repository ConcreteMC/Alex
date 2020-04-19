namespace Alex.Blocks.Minecraft
{
    public class Bell : Block
    {
        public Bell()
        {
            Transparent = true;
            Solid = true;
            IsFullBlock = false;
            IsFullCube = false;

            BlockMaterial = Material.Iron;
        }
    }
}