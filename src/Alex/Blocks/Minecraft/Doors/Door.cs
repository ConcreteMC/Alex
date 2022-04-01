using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Common;
using Alex.Common.Blocks;
using Alex.Common.Blocks.Properties;
using Alex.Common.Utils.Vectors;
using Alex.Entities;
using Alex.Interfaces;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Blocks.Minecraft.Doors
{
	public class Door : OpenableBlockBase
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Door));

		private static PropertyBool UPPER = new PropertyBool("half", "upper", "lower");
		private static PropertyBool RIGHTHINCHED = new PropertyBool("hinge", "right", "left");
		private static PropertyFace FACING = new PropertyFace("facing");

		public bool IsUpper => UPPER.GetValue(BlockState);
		public bool IsHinchOnTheRight => RIGHTHINCHED.GetValue(BlockState);
		public BlockFace FacingDirection => FACING.GetValue(BlockState);

		protected bool CanOpen { get; set; } = true;

		public Door(uint blockId) : base()
		{
			Transparent = true;
			RequiresUpdate = true;
			CanInteract = true;
			IsFullCube = true;
		}

		private BlockState Update(IBlockAccess world,
			BlockState blockState,
			BlockCoordinates coordinates,
			BlockCoordinates updated)
		{
			var updatedBlock = world.GetBlockState(updated);

			if (!(updatedBlock.Block is Door doorBlock))
				return blockState;

			bool isUpper = IsUpper;

			if (updated == coordinates + BlockCoordinates.Up && !isUpper)
			{
				blockState = blockState.WithProperty(RIGHTHINCHED, doorBlock.IsHinchOnTheRight);
			}
			else if (updated == coordinates + BlockCoordinates.Down && isUpper)
			{
				blockState = blockState.WithProperty(OPEN, doorBlock.IsOpen);
				blockState = blockState.WithProperty(FACING, doorBlock.FacingDirection);
			}

			return blockState;
		}

		public override BlockState BlockPlaced(IBlockAccess world, BlockState state, BlockCoordinates position)
		{
			/*if (!IsUpper)
			{
				var blockstate = state.WithProperty(UPPER, true);
				world.SetBlockState(position.X, position.Y + 1, position.Z, blockstate, 0, BlockUpdatePriority.High);

				return state;
			}*/

			if (IsUpper)
			{
				return Update(world, state, position, position + BlockCoordinates.Down);
			}

			return Update(world, state, position, position + BlockCoordinates.Up);
		}

		public override void BlockUpdate(World world, BlockCoordinates position, BlockCoordinates updatedBlock)
		{
			var newValue = Update(world, BlockState, position, updatedBlock);

			if (newValue.Id != BlockState.Id)
			{
				world.SetBlockState(position.X, position.Y, position.Z, newValue);
			}
		}

		/// <inheritdoc />
		public override bool PlaceBlock(World world,
			Player player,
			BlockCoordinates position,
			BlockFace face,
			Vector3 cursorPosition)
		{
			position += face.GetBlockCoordinates();
			var blockAbove = world.GetBlockState(position + BlockCoordinates.Up);

			if (!blockAbove.Block.BlockMaterial.IsReplaceable)
				return true;

			var facing = player.KnownPosition.GetFacing();
			BlockState state = BlockState;
			state = state.WithProperty(FACING, facing);
			state = state.WithProperty(UPPER, false);

			var blockLeft = world.GetBlockState(position + BlockCoordinates.Left);
			var blockRight = world.GetBlockState(position + BlockCoordinates.Right);

			if (blockLeft.Block is Door leftDoor)
			{
				state = state.WithProperty(RIGHTHINCHED, true);
			}
			else if (blockRight.Block is Door)
			{
				state = state.WithProperty(RIGHTHINCHED, false);
			}

			world.SetBlockState(position, state);

			return true;
			//return state;
		}

		public override bool ShouldRenderFace(BlockFace face, Block neighbor)
		{
			return true;
		}

		public override bool TryGetStateProperty(string prop, out IStateProperty stateProperty)
		{
			switch (prop)
			{
				case "half":
					stateProperty = UPPER;

					return true;

				case "hinge":
					stateProperty = RIGHTHINCHED;

					return true;
			}

			return base.TryGetStateProperty(prop, out stateProperty);
		}
	}
}