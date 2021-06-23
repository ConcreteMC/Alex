using Alex.Blocks.Properties;
using Alex.Blocks.State;
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
			int myLevelValue = 0;
			BlockState.TryGetValue(LEVEL, out myLevelValue);//.GetTypedValue(LEVEL);

			if (neighbor.BlockMaterial == Material.WaterPlant
			    || neighbor.BlockMaterial == Material.ReplaceableWaterPlant)
				return false;
			
			if (neighbor.BlockMaterial.IsLiquid || neighbor is LiquidBlock)
			{
				int neighborLevel = 0;
				neighbor.BlockState.TryGetValue(LEVEL, out myLevelValue);
				//var neighborLevel = neighbor.BlockState.GetTypedValue(LEVEL);

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

		/// <inheritdoc />
		public override bool TryGetStateProperty(string prop, out StateProperty stateProperty)
		{
			switch (prop)
			{
				case "level":
					stateProperty = LEVEL;
					return true;
			}
			
			return base.TryGetStateProperty(prop, out stateProperty);
		}
	}
}