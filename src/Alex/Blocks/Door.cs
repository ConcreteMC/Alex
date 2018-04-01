using System;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Entities;
using Alex.ResourcePackLib.Json;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Blocks
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
		}

		public override void BlockPlaced(IWorld world, BlockCoordinates position)
		{
			if (IsUpper)
			{
				Block below = (Block) world.GetBlock(position - new BlockCoordinates(0, 1, 0));
				if (below is Door bottom)
				{
					if (bottom.IsOpen)
					{
						BlockState state = (BlockState)BlockState.WithProperty(OPEN, true);
						world.SetBlockState(position.X, position.Y, position.Z, state);
					}
				}
			}
			else
			{
				Block up = (Block)world.GetBlock(position + new BlockCoordinates(0, 1, 0));
				if (up is Door upper)
				{
					if (upper.IsUpper)
					{
						BlockState state = (BlockState) BlockState.WithProperty(UPPER, false);
						world.SetBlockState(position.X, position.Y, position.Z, state);
					}
				}
			}
			//return false;
		}

		public override void Interact(IWorld world, BlockCoordinates position, BlockFace face, Entity sourceEntity)
		{
			if (!IsUpper)
			{
				BlockState state = (BlockState)BlockState.WithProperty(OPEN, !IsOpen);
				world.SetBlockState(position.X, position.Y, position.Z, state);
			}
		}

		public override void BlockUpdate(IWorld world, BlockCoordinates position, BlockCoordinates updatedBlock)
		{
			if (IsUpper && updatedBlock.Y < position.Y)
			{
				var changedBlock = world.GetBlockState(updatedBlock.X, updatedBlock.Y, updatedBlock.Z);
				if (!changedBlock.GetTypedValue(UPPER))
				{
					var myMeta = (BlockState) BlockState.WithProperty(OPEN, changedBlock.GetTypedValue(OPEN));
					world.SetBlockState(position.X, position.Y, position.Z, myMeta);
				}
			}
			Log.Info($"Door blockupdate called!");
		}
	}
}
