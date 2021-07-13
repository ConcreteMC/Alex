using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Logs
{
    public class Log : Block
    {
        public Log()
        {
            Transparent = false;
            Solid = true;

            BlockMaterial = Material.Wood.Clone().WithHardness(2);
           // Hardness = 2;
        }
    }
}