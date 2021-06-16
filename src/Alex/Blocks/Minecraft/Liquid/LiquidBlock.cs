using Alex.Blocks.Properties;
using Alex.Common.Blocks;

namespace Alex.Blocks.Minecraft.Liquid
{
	public class LiquidBlock : Block
	{
		public static readonly PropertyInt LEVEL = new PropertyInt("level", 0);
		protected LiquidBlock()
		{
			
		}

		public override bool ShouldRenderFace(BlockFace face, Block neighbor)
		{
			var myLevelValue = BlockState.GetTypedValue(LEVEL);

			if (neighbor.BlockMaterial == Material.WaterPlant
			    || neighbor.BlockMaterial == Material.ReplaceableWaterPlant)
				return false;
			
			if (neighbor.BlockMaterial.IsLiquid || neighbor is LiquidBlock)
			{
				var neighborLevel = neighbor.BlockState.GetTypedValue(LEVEL);

				if (neighborLevel < myLevelValue)
				{
					return true;
				}

				return false;
			}

			if (neighbor.Solid && (!neighbor.Transparent || neighbor.BlockMaterial.IsOpaque))
				return false;

			if (neighbor.Solid && neighbor.Transparent && !neighbor.IsFullCube)
				return true;

			//else if (neighbor.Transparent)
			return base.ShouldRenderFace(face, neighbor);
		}
	}
}