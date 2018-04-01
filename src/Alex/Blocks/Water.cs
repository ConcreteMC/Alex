using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Properties;
using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Water : Block
	{
		public static readonly PropertyInt LEVEL = new PropertyInt("level", 0);
		public Water() : base(34)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = true;
			HasHitbox = false;
			//BlockModel = BlockFactory.StationairyWaterModel;

			IsWater = true;
			IsSourceBlock = true;
			//BlockMaterial = Material.Water;
		}

		public override void BlockPlaced(IWorld world, BlockCoordinates position)
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
		}
	}
}
