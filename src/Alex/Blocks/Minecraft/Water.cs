using Alex.API.Blocks;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Properties;

namespace Alex.Blocks.Minecraft
{
	public class Water : Block
	{
		public static readonly PropertyInt LEVEL = new PropertyInt("level", 0);
		public Water() : base(34)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = true;
			HasHitbox = true;
			//BlockModel = BlockFactory.StationairyWaterModel;

			IsWater = true;
			IsSourceBlock = true;
			Animated = true;
			BlockMaterial = Material.Water;

			LightOpacity = 3;
		}
		
		public override bool ShouldRenderFace(BlockFace face, Block neighbor)
		{
			if (neighbor.BlockMaterial == Material.Water || neighbor.BlockMaterial == Material.WaterPlant)
			{
				var neighborLevel = neighbor.BlockState.GetTypedValue(LEVEL);

				if (neighborLevel < BlockState.GetTypedValue(LEVEL))
				{
					return true;
				}

				return false;
			}

			if (neighbor.IsWater)
				return false;

			if (neighbor.Solid && (!neighbor.Transparent || neighbor.BlockMaterial.IsOpaque))
				return false;

			if (neighbor.Solid && neighbor.Transparent && !neighbor.IsFullCube)
				return true;
			
			//else if (neighbor.Transparent)
			return base.ShouldRenderFace(face, neighbor);
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
