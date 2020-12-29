namespace Alex.Blocks.Minecraft
{
	public class WallSign : Block
	{
		public WoodType WoodType { get; }
		
		public WallSign(WoodType woodType = WoodType.Oak) : base()
		{
			WoodType = woodType;
			
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			Renderable = false;
			CanInteract = true;
			
			HasHitbox = true;

			//RequiresUpdate = true;
			
			Hardness = 1;
			BlockMaterial = Material.Wood;
		}
	}

	public class StandingSign : Block
	{
		public WoodType WoodType { get; }
		public StandingSign(WoodType woodType = WoodType.Oak)
		{
			WoodType = woodType;
			
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			Renderable = false;
			CanInteract = true;
			
			HasHitbox = true;

			//RequiresUpdate = true;
			
			Hardness = 1;
			BlockMaterial = Material.Wood;
		}
	}

	public enum WoodType
	{
		Oak,
		Spruce,
		Birch,
		Jungle,
		Acacia,
		DarkOak,
		Crimson,
		Warped
	}
}
