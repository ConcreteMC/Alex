namespace Alex.Blocks.Minecraft
{
	public class WallSign : Block
	{
		public WallSign() : base()
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			Renderable = false;
			
			HasHitbox = true;

			//RequiresUpdate = true;
			
			Hardness = 1;
			BlockMaterial = Material.Wood;
		}
	}

	public class StandingSign : Block
	{
		public StandingSign()
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			Renderable = false;
			
			HasHitbox = true;

			//RequiresUpdate = true;
			
			Hardness = 1;
			BlockMaterial = Material.Wood;
		}
	}
}
