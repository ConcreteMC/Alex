using Alex.Blocks.Materials;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Common.Utils.Vectors;
using Alex.Entities;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;

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

        /// <inheritdoc />
        public override BlockState PlaceBlock(World world, Player player, BlockCoordinates position, BlockFace face, Vector3 cursorPosition)
        {
            if (face == BlockFace.Up || face == BlockFace.Down)
            {
                return BlockState.WithProperty("axis", "y");
            }
            else if (face == BlockFace.East || face == BlockFace.West)
            {
                return BlockState.WithProperty("axis", "x");
            }
            else if (face == BlockFace.North || face == BlockFace.South)
            {
                return BlockState.WithProperty("axis", "z");
            }
            return base.PlaceBlock(world, player, position, face, cursorPosition);
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