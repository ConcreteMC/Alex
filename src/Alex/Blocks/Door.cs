using System;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.ResourcePackLib.Json;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Blocks
{
	public class Door : Block
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Door));

		public static PropertyBool UPPER = new PropertyBool("half", "upper", "lower");
		public static PropertyBool OPEN = new PropertyBool("open");
		public static PropertyBool POWERED = new PropertyBool("powered");
		public static PropertyFace FACING = new PropertyFace("facing");

		public bool IsUpper => BlockState.GetTypedValue(UPPER);//(Metadata & 0x08) == 0x08;
		public bool IsOpen => BlockState.GetTypedValue(OPEN);// (Metadata & 0x04) == 0x04;
		//public bool IsRightHinch => (Metadata & 0x01) == 0x01;
		public bool IsPowered => BlockState.GetTypedValue<bool>(POWERED); //(Metadata & 0x02) == 0x02;

		public Door(int blockId, byte metadata) : base(blockId, metadata)
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
					//if (upper.IsRightHinch)
					{
				
					}
				}
			}
			//return false;
		}
	}
}
