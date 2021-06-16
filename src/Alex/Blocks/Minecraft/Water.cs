using Alex.Blocks.Properties;
using Alex.Common.Blocks;

namespace Alex.Blocks.Minecraft
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
	
	public class Water : LiquidBlock
	{
		public Water() : base()
		{
			Solid = false;
			Transparent = true;
			HasHitbox = true;
			//BlockModel = BlockFactory.StationairyWaterModel;

			//IsWater = true;
			base.BlockMaterial = Material.Water;

			LightOpacity = 3;
		}
		


		/*public override void BlockPlaced(IWorld world, BlockCoordinates position)
		{
			if (BlockState != null)
			{
				if (BlockState.GetTypedValue(LEVEL) == 0)
				{
					IsSourceBlock = true;
				}
				else
				{
					IsSourceBlock = false;
				}
			}
			base.BlockPlaced(world, position);
		}*/
	}
}
