using Alex.Blocks.Materials;
using Alex.Blocks.Minecraft.Signs;
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
		public override bool PlaceBlock(World world,
			Player player,
			BlockCoordinates position,
			BlockFace face,
			Vector3 cursorPosition)
		{
			position += face.GetBlockCoordinates();
			BlockState state = BlockState;

			if (face == BlockFace.Up || face == BlockFace.Down)
			{
				state = BlockState.WithProperty("axis", "y");
			}
			else if (face == BlockFace.East || face == BlockFace.West)
			{
				state = BlockState.WithProperty("axis", "x");
			}
			else if (face == BlockFace.North || face == BlockFace.South)
			{
				state = BlockState.WithProperty("axis", "z");
			}

			world.SetBlockState(position, state);

			return true;
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