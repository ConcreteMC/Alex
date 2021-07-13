using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft.Logs
{
    public class Log : Block
    {
        public Log(WoodType woodType = WoodType.Oak)
        {
            Transparent = false;
            Solid = true;

            base.BlockMaterial = Material.Wood.Clone().WithHardness(2).WithMapColor(woodType.ToMapColor());
           // Hardness = 2;
        }
    }

    public class BirchLog : Log
    {
        public BirchLog()
        {
            base.BlockMaterial = Material.Wood.Clone().WithHardness(2).WithMapColor(MapColor.Sand);
        }
    }
}