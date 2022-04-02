using Alex.Blocks.Materials;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Common.Blocks.Properties;
using Alex.Interfaces;

namespace Alex.Blocks.Minecraft.Liquid
{
	public class LiquidBlock : Block
	{
		public static readonly PropertyInt LEVEL = new PropertyInt("level", 0);

		protected LiquidBlock() { }

		private int GetLevel(BlockState state)
		{
			int neighborLevel = LEVEL.GetValue(state);
			
			if (neighborLevel == 0)
				neighborLevel = 8;

			return neighborLevel;
		}
		
		public override bool ShouldRenderFace(BlockFace face, Block neighbor)
		{
			if (face == BlockFace.Up && (neighbor.BlockMaterial.IsLiquid || neighbor is LiquidBlock))
				return false;
			
			if (neighbor.BlockMaterial == Material.WaterPlant
			    || neighbor.BlockMaterial == Material.ReplaceableWaterPlant)
				return false;

			if (neighbor.BlockMaterial.IsLiquid || neighbor is LiquidBlock)
			{
				int myLevelValue = GetLevel(BlockState);//LEVEL.GetValue(BlockState);
				int neighborLevel = GetLevel(BlockState);

				if (neighborLevel == myLevelValue)
					return false;
				
				//var neighborLevel = neighbor.BlockState.GetTypedValue(LEVEL);

				if (neighborLevel > myLevelValue)
				{
					return true;
				}

				return false;
			}
			
			if (neighbor.Solid && neighbor.Transparent && !neighbor.IsFullCube)
				return true;
			
			if (neighbor.Solid && (!neighbor.Transparent || neighbor.BlockMaterial.IsOpaque))
				return false;

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