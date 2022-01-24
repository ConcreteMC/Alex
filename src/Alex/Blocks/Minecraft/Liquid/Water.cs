using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Liquid
{
	public class Water : LiquidBlock
	{
		public Water() : base()
		{
			Solid = false;
			Transparent = true;
			HasHitbox = false;
			//BlockModel = BlockFactory.StationairyWaterModel;

			//IsWater = true;
			base.BlockMaterial = Material.Water;

			Diffusion = 3;
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