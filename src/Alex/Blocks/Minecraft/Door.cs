using System;
using Alex.API.Blocks.State;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Properties;
using Alex.Entities;
using Alex.ResourcePackLib.Json;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Blocks.Minecraft
{
	public class Door : Block
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Door));

		/*public static PropertyBool UPPER = new PropertyBool("half", "upper", "lower");
		public static PropertyBool OPEN = new PropertyBool("open");
		public static PropertyBool POWERED = new PropertyBool("powered");
		public static PropertyFace FACING = new PropertyFace("facing");*/

		private static PropertyBool OPEN = new PropertyBool("open");
		private static PropertyBool UPPER = new PropertyBool("half", "upper", "lower");
		private static PropertyBool RIGHTHINCHED = new PropertyBool("hinge", "left", "right");
		private static PropertyBool POWERED = new PropertyBool("powered");
		private static PropertyFace FACING = new PropertyFace("facing");

		public bool IsUpper => BlockState.GetTypedValue(UPPER);//(Metadata & 0x08) == 0x08;
		public bool IsOpen => BlockState.GetTypedValue(OPEN);// (Metadata & 0x04) == 0x04;
		//public bool IsRightHinch => (Metadata & 0x01) == 0x01;
		public bool IsPowered => BlockState.GetTypedValue<bool>(POWERED); //(Metadata & 0x02) == 0x02;

		protected bool CanOpen { get; set; } = true;
		public Door(uint blockId) : base(blockId)
		{
			Transparent = true;
			RequiresUpdate = true;
		}

		private IBlockState Update(IWorld world, IBlockState blockState, BlockCoordinates coordinates, BlockCoordinates updated)
		{
			var updatedBlock = world.GetBlockState(updated);
			if (!(updatedBlock.Block is Door))
				return blockState;

			bool isUpper = false;
			if (blockState.TryGetValue("half", out string half))
			{
				isUpper = half.Equals("upper", StringComparison.InvariantCultureIgnoreCase);
			}
			
			if (updated == coordinates + BlockCoordinates.Up && !isUpper)
			{
				if (updatedBlock.TryGetValue("hinge", out var hingeValue))
				{
					blockState = blockState.WithProperty("hinge", hingeValue, false, "half", "open", "facing");
				}
			}
			else if (updated == coordinates + BlockCoordinates.Down && isUpper)
			{
				if (updatedBlock.TryGetValue("open", out string open))
				{
					blockState = blockState.WithProperty("open", open, false, "half", "hinge");
				}

				if (updatedBlock.TryGetValue("facing", out var facing))
				{
					blockState = blockState.WithProperty("facing",
						facing, false, "half", "hinge", "open");
				}
			}

			return blockState;
		}

		public override IBlockState BlockPlaced(IWorld world, IBlockState state, BlockCoordinates position)
		{
			if (state.TryGetValue("half", out string half) && half.Equals("upper", StringComparison.InvariantCultureIgnoreCase))
			{
				return Update(world, state, position, position + BlockCoordinates.Down);
			}
			else
			{
				return Update(world, state, position, position + BlockCoordinates.Up);
			}
			
			return base.BlockPlaced(world, state, position);
		}

		public override void Interact(IWorld world, BlockCoordinates position, BlockFace face, Entity sourceEntity)
		{
			/*if (!IsUpper)
			{
				BlockState state = (BlockState)BlockState.WithProperty(OPEN, (!IsOpen).ToString());
				world.SetBlockState(position.X, position.Y, position.Z, state);
			}*/
		}

		public override void BlockUpdate(IWorld world, BlockCoordinates position, BlockCoordinates updatedBlock)
		{
			//if (updatedBlock != position - new BlockCoordinates(0, 1, 0))
			//	return;
			
			var newValue = Update(world, BlockState, position, updatedBlock);
			if (newValue != BlockState)
			{
				world.SetBlockState(position.X, position.Y, position.Z, newValue);
			}
			/*if (IsUpper && updatedBlock.Y < position.Y)
			{
				var changedBlock = world.GetBlockState(updatedBlock.X, updatedBlock.Y, updatedBlock.Z);
				if (!changedBlock.GetTypedValue(UPPER))
				{
					var myMeta = (BlockState) BlockState.WithProperty(OPEN, (changedBlock.GetTypedValue(OPEN)).ToString());
					world.SetBlockState(position.X, position.Y, position.Z, myMeta);
				}
			}
			Log.Info($"Door blockupdate called!");*/
		}
	}
}
