namespace Alex.Blocks.Minecraft
{
    public class Log : Block
    {
        public Log()
        {
            Transparent = false;
            Solid = true;

            BlockMaterial = Material.Wood;
            Hardness = 2;
        }
    }
}