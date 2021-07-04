using System.Collections.Generic;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
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
			
			VariantMapper =
				new BlockStateVariantMapper(new List<BlockState>()
				{
					this
				});

			VariantMapper.Model = new ResourcePackBlockModel(Alex.Instance.Resources);
		}
	}
}