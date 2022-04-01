using Alex.Blocks.Materials;
using Alex.Blocks.Properties;
using Alex.Common.Blocks;
using Alex.Common.Blocks.Properties;
using Alex.Interfaces;

namespace Alex.Blocks.Minecraft.Liquid
{
	public class LiquidBlock : Block
	{
		public static readonly PropertyInt LEVEL = new PropertyInt("level", 0);

		protected LiquidBlock() { }

		public override bool ShouldRenderFace(BlockFace face, Block neighbor)
		{
			int myLevelValue = LEVEL.GetValue(BlockState);

			if (neighbor.BlockMaterial == Material.WaterPlant
			    || neighbor.BlockMaterial == Material.ReplaceableWaterPlant)
				return false;

			if (neighbor.BlockMaterial.IsLiquid || neighbor is LiquidBlock)
			{
				int neighborLevel = LEVEL.GetValue(neighbor.BlockState);
				//var neighborLevel = neighbor.BlockState.GetTypedValue(LEVEL);

				if (neighborLevel > myLevelValue)
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
		public override bool TryGetStateProperty(string prop, out IStateProperty stateProperty)
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