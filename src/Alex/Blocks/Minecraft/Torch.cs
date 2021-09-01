using Alex.Blocks.Materials;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Common.Utils.Vectors;
using Alex.Entities;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Blocks.Minecraft
{
	public class Torch : Block
	{
		public Torch(bool wallTorch = false) : base()
		{
			Solid = false;
			Transparent = true;

			IsFullCube = false;
			
			Luminance = 14;

			BlockMaterial = Material.Decoration;
		}

		/// <inheritdoc />
		public override bool PlaceBlock(World world, Player player, BlockCoordinates position, BlockFace face, Vector3 cursorPosition)
		{
			position += face.GetBlockCoordinates();
			BlockState state = BlockState;
			if (face != BlockFace.Up && face != BlockFace.Down)
			{
				var wallTorch = BlockFactory.GetBlockState("minecraft:wall_torch");

				if (wallTorch != null)
				{
					state = wallTorch.WithProperty(Facing, face);
				}
			}

			world.SetBlockState(position, state);
			return true;
		}
	}
}
