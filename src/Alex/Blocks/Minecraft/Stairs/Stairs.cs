using System;
using Alex.Blocks.Materials;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Common.Blocks.Properties;
using Alex.Common.Utils.Vectors;
using Alex.Entities;
using Alex.Graphics.Models.Blocks;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Blocks.Minecraft.Stairs
{
	public class WoodStairs : Stairs
	{
		public WoodStairs() : base()
		{
			BlockMaterial = Material.Wood.Clone().SetWaterLoggable().WithHardness(2);
		}
	}

	public class Stairs : Block
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Stairs));

		public Stairs(uint baseId) : base()
		{
			Solid = true;
			Transparent = true;
			RequiresUpdate = true;
			//Hardness = 2;

			BlockMaterial = Material.Stone.Clone().SetWaterLoggable().WithHardness(2);
		}

		public Stairs()
		{
			Solid = true;
			Transparent = true;
			RequiresUpdate = true;
			// Hardness = 2;

			BlockMaterial = Material.Stone.Clone().SetWaterLoggable().WithHardness(2);
		}

		/// <inheritdoc />
		public override bool ShouldRenderFace(BlockFace face, Block neighbor)
		{
			var facing = GetFacing(BlockState);

			if (facing != face || face == BlockFace.None)
				return true;

			return base.ShouldRenderFace(face, neighbor);
		}

		protected static string GetHalf(BlockState state)
		{
			if (state.TryGetValue("half", out string facingValue))
			{
				return facingValue;
			}

			return string.Empty;
		}

		private bool UpdateState(IBlockAccess world,
			BlockState state,
			BlockCoordinates position,
			BlockCoordinates updatedBlock,
			out BlockState result,
			bool checkCorners)
		{
			result = state;
			var block = world.GetBlockState(updatedBlock).Block;

			if (!(block is Stairs))
			{
				return false;
			}

			var myHalf = GetHalf(state);

			var blockState = block.BlockState;

			if (myHalf != GetHalf(blockState))
				return false;

			var facing = GetFacing(state);
			var neighborFacing = GetFacing(blockState);

			if (checkCorners)
			{
				BlockCoordinates offset1 = facing.GetVector3();

				if (neighborFacing != facing && neighborFacing != facing.Opposite()
				                             && updatedBlock == position + offset1)
				{
					if (neighborFacing == BlockModel.RotateDirection(
						    facing, 1, BlockModel.FACE_ROTATION, BlockModel.INVALID_FACE_ROTATION))
					{
						if (facing == BlockFace.North || facing == BlockFace.South)
						{
							result = state.WithProperty("shape", "outer_right");
						}
						else
						{
							result = state.WithProperty("shape", "outer_right");
						}

						return true;
					}

					if (facing == BlockFace.North || facing == BlockFace.South)
					{
						result = state.WithProperty("shape", "outer_left");
					}
					else
					{
						result = state.WithProperty("shape", "outer_left");
					}

					return true;
				}

				BlockCoordinates offset2 = facing.Opposite().GetVector3();

				if (neighborFacing != facing && neighborFacing != facing.Opposite()
				                             && updatedBlock == position + offset2)
				{
					if (neighborFacing == BlockModel.RotateDirection(
						    facing, 1, BlockModel.FACE_ROTATION, BlockModel.INVALID_FACE_ROTATION))
					{
						if (facing == BlockFace.North || facing == BlockFace.South)
						{
							result = state.WithProperty("shape", "inner_right");
						}
						else
						{
							result = state.WithProperty("shape", "inner_right");
						}

						return true;
					}

					if (facing == BlockFace.North || facing == BlockFace.South)
					{
						result = state.WithProperty("shape", "inner_left");
					}
					else
					{
						result = state.WithProperty("shape", "inner_left");
					}

					return true;
				}
			}
			else
			{
				if (facing == neighborFacing)
				{
					result = state.WithProperty("shape", "straight");

					return true;
				}
			}


			return false;
		}

		public override void BlockUpdate(World world, BlockCoordinates position, BlockCoordinates updatedBlock)
		{
			if (UpdateState(world, BlockState, position, updatedBlock, out var state, true))
			{
				world.SetBlockState(position.X, position.Y, position.Z, state);
			}
		}

		public override BlockState BlockPlaced(IBlockAccess world, BlockState state, BlockCoordinates position)
		{
			if (UpdateState(world, state, position, position + BlockCoordinates.Forwards, out state, true)
			    || UpdateState(world, state, position, position + BlockCoordinates.Backwards, out state, true)
			    || UpdateState(world, state, position, position + BlockCoordinates.Left, out state, true)
			    || UpdateState(world, state, position, position + BlockCoordinates.Right, out state, true))
			{
				return state;
			}

			return state.WithProperty("shape", "straight");
		}

		/// <inheritdoc />
		public override bool PlaceBlock(World world,
			Player player,
			BlockCoordinates position,
			BlockFace face,
			Vector3 cursorPosition)
		{
			position += face.GetBlockCoordinates();
			var upsideDown = ((cursorPosition.Y > 0.5 && face != BlockFace.Up) || face == BlockFace.Down);
			var blockState = BlockState;

			blockState = blockState.WithProperty("half", upsideDown ? "top" : "bottom");

			if (face == BlockFace.Up || face == BlockFace.Down)
			{
				face = player.KnownPosition.GetFacing();
			}
			else
			{
				face = face.Opposite();
			}

			blockState = blockState.WithProperty(Facing, face);

			world.SetBlockState(position, blockState);

			return true;

			//  return base.PlaceBlock(world, player, position, face, cursorPosition);
		}

		/// <inheritdoc />
		public override bool TryGetStateProperty(string prop, out IStateProperty stateProperty)
		{
			return base.TryGetStateProperty(prop, out stateProperty);
		}
	}
}