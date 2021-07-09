using System;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Common.Blocks.Properties;
using Alex.Common.Utils.Vectors;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using NLog;

namespace Alex.Blocks.Minecraft.Doors
{
	public class Door : OpenableBlockBase
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Door));

		private static PropertyBool UPPER = new PropertyBool("half", "upper", "lower");
		private static PropertyBool RIGHTHINCHED = new PropertyBool("hinge", "left", "right");
		private static PropertyFace FACING = new PropertyFace("facing");

		public bool IsUpper => UPPER.GetValue(BlockState);
		protected bool CanOpen { get; set; } = true;
		public Door(uint blockId) : base()
		{
			Transparent = true;
			RequiresUpdate = true;
			CanInteract = true;
			IsFullCube = true;
		}

		private BlockState Update(IBlockAccess world, BlockState blockState, BlockCoordinates coordinates, BlockCoordinates updated)
		{
			var updatedBlock = world.GetBlockState(updated);
			if (!(updatedBlock.Block is Door))
				return blockState;

			bool isUpper = IsUpper;
			
			if (updated == coordinates + BlockCoordinates.Up && !isUpper)
			{
				if (updatedBlock.TryGetValue("hinge", out var hingeValue))
				{
					blockState = blockState.WithProperty("hinge", hingeValue);
				}
			}
			else if (updated == coordinates + BlockCoordinates.Down && isUpper)
			{
				if (updatedBlock.TryGetValue("open", out string open))
				{
					blockState = blockState.WithProperty("open", open);
				}

				if (updatedBlock.TryGetValue("facing", out var facing))
				{
					blockState = blockState.WithProperty("facing",
						facing);
				}
			}

			return blockState;
		}

		public override BlockState BlockPlaced(IBlockAccess world, BlockState state, BlockCoordinates position)
		{
			if (state.TryGetValue("half", out string half) && half.Equals(
				"upper", StringComparison.OrdinalIgnoreCase))
			{
				return Update(world, state, position, position + BlockCoordinates.Down);
			}

			return Update(world, state, position, position + BlockCoordinates.Up);
		}

		public override void BlockUpdate(World world, BlockCoordinates position, BlockCoordinates updatedBlock)
		{
			var newValue = Update(world, BlockState, position, updatedBlock);
			if (newValue != BlockState)
			{
				world.SetBlockState(position.X, position.Y, position.Z, newValue);
			}
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
			}
			return base.TryGetStateProperty(prop, out stateProperty);
		}
	}
}
