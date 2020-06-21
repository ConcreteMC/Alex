using System;
using Alex.API.Blocks;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Entities;
using Alex.ResourcePackLib.Json;
using Alex.Worlds;

namespace Alex.Blocks.Minecraft
{
	public class WoodenDoor : Block
	{
		private static PropertyBool OPEN = new PropertyBool("open");
		private static PropertyBool UPPER = new PropertyBool("half", "upper", "lower");
		private static PropertyBool RIGHTHINCHED = new PropertyBool("hinge", "left", "right");
		private static PropertyBool POWERED = new PropertyBool("powered");
		private static PropertyFace FACING = new PropertyFace("facing");

	//	public bool IsOpen => (Metadata & 0x04) == 0x04;
	//	public bool IsUpper => (Metadata & 0x08) == 0x08;
	//	public bool IsRightHinch => (Metadata & 0x01) == 0x01;
	//	public bool IsPowered => (Metadata & 0x02) == 0x02;

		public WoodenDoor(byte meta) : this(64, meta)
		{

		}

		public WoodenDoor(int blockId, byte meta) : base(blockId, meta)
		{
			Transparent = true;
		}

		private void Toggle(World world, BlockCoordinates position)
		{
		/*	if (IsUpper)
			{
				Block below = (Block)world.GetBlock(position - new BlockCoordinates(0, 1, 0));
				if (below is WoodenDoor bottom && !bottom.IsUpper)
				{
					bool open = !bottom.BlockStateResource.GetTypedValue(OPEN);

					IBlockState state = BlockStateResource.Clone().WithProperty(OPEN, open).WithProperty(UPPER, true);
					world.SetBlock(position.X, position.Y, position.Z, BlockFactory.GetBlock(BlockId, GetMetaFromState(state)));

					IBlockState state2 = bottom.BlockStateResource.Clone().WithProperty(RIGHTHINCHED, IsRightHinch).WithProperty(UPPER, false)
						.WithProperty(OPEN, open);
					world.SetBlock(position.X, position.Y - 1, position.Z, BlockFactory.GetBlock(BlockId, GetMetaFromState(state2)));
				}
			}
			else
			{
				Block up = (Block)world.GetBlock(position + new BlockCoordinates(0, 1, 0));
				if (up is WoodenDoor upper && upper.IsUpper)
				{
					bool open = !BlockStateResource.GetTypedValue(OPEN);
					IBlockState state = BlockStateResource.Clone().WithProperty(RIGHTHINCHED, upper.IsRightHinch).WithProperty(UPPER, false).WithProperty(OPEN, open);
					world.SetBlock(position.X, position.Y, position.Z, BlockFactory.GetBlock(BlockId, GetMetaFromState(state)));

					IBlockState state2 = upper.BlockStateResource.Clone().WithProperty(OPEN, open).WithProperty(UPPER, true);
					world.SetBlock(position.X, position.Y + 1, position.Z, BlockFactory.GetBlock(BlockId, GetMetaFromState(state2)));
				}
			}*/
		}

		public override void Interact(World world, BlockCoordinates position, BlockFace face, Entity sourceEntity)
		{
			Toggle(world, position);
		}
		
		public byte GetMetaFromState(IBlockState state)
		{
			byte i = 0;

			if (state.GetTypedValue(UPPER))
			{
				i = (byte) (i | 8);

				if (state.GetTypedValue(RIGHTHINCHED))
				{
					i |= 1;
				}

				if (state.GetTypedValue(POWERED))
				{
					i |= 2;
				}
			}
			else
			{
				var facingValue = Correct(state.GetTypedValue(FACING));

				i = (byte) ((i & 245) + facingValue);

				if (state.GetTypedValue(OPEN))
				{
					i |= 4;
				}
			}

			return i;
		}

		private static BlockFace[] HORIZONTALS = new BlockFace[4]
		{
			BlockFace.North,
			BlockFace.East,
			BlockFace.South,
			BlockFace.West
		};

		private int Correct(BlockFace face)
		{
			switch (face)
			{
				case BlockFace.East:
					return 0;
				case BlockFace.West:
					return 2;
				case BlockFace.North:
					return 3;
				case BlockFace.South:
					return 1;
			}

			return 0;
		}

		public static BlockFace GetHorizontal(int horizontalIndexIn)
		{
			return HORIZONTALS[Math.Abs(horizontalIndexIn % 4)];
		}
	}
}
