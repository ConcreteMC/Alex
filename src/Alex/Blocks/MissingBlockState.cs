using System;
using System.Collections.Generic;
using Alex.Blocks.Minecraft;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Common.Resources;
using Alex.Graphics.Models.Blocks;
using Alex.ResourcePackLib.Json.BlockStates;

namespace Alex.Blocks
{
	public class MissingBlockState : BlockState
	{
		public MissingBlockState(string name)
		{
			Name = name;

			Block = new MissingBlock();
			Block.BlockState = this;

			ModelData = new BlockStateVariant()
			{
				new BlockStateModel()
				{
					Uvlock = false,
					Weight = 0,
					X = 0,
					Y = 0,
					ModelName = new ResourceLocation("minecraft:block/cube")
				}
			};

			VariantMapper = new BlockStateVariantMapper(new List<BlockState>() { this });

			VariantMapper.Model = new ResourcePackBlockModel(Alex.Instance.Resources);
		}
	}

	public class ItemFrameBlockState : BlockState
	{
		private ItemFrameBlockState(bool map, BlockFace facing)
		{
			Name = "minecraft:item_frame";

			Block = new ItemFrame();
			Block.BlockState = this;

			States.Add(Block.Facing.WithValue(facing));
			States.Add(new PropertyBool("map").WithValue(map));

			int y = 0;

			switch (facing)
			{
				case BlockFace.East:
					y = 90;

					break;

				case BlockFace.West:
					y = 270;

					break;

				case BlockFace.North:
					y = 0;

					break;

				case BlockFace.South:
					y = 180;

					break;
			}

			ModelData = new BlockStateVariant()
			{
				new BlockStateModel()
				{
					ModelName = new ResourceLocation(
						map ? "minecraft:block/item_frame_map" : "minecraft:block/item_frame"),
					Y = y
				}
			};
		}

		public static BlockStateVariantMapper Build()
		{
			var blockStates = new List<BlockState>()
			{
				new ItemFrameBlockState(false, BlockFace.North) { Default = true, },
				new ItemFrameBlockState(true, BlockFace.North),
				new ItemFrameBlockState(false, BlockFace.East),
				new ItemFrameBlockState(true, BlockFace.East),
				new ItemFrameBlockState(false, BlockFace.South),
				new ItemFrameBlockState(true, BlockFace.South),
				new ItemFrameBlockState(false, BlockFace.West),
				new ItemFrameBlockState(true, BlockFace.West),
			};

			var mapper = new BlockStateVariantMapper(blockStates);
			mapper.Model = new ResourcePackBlockModel(Alex.Instance.Resources);

			return mapper;
		}
	}
}