namespace Alex.Blocks.Minecraft
{
	public class Skull : Block
	{
		public SkullType SkullType { get; set; } = SkullType.Player;
		public Skull()
		{
			Renderable = false;
			HasHitbox = true;
			
			
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}

	public class WallSkull : Skull
	{
		
	}

	public enum SkullType
	{
		Player,
		Skeleton,
		WitherSkeleton,
		Zombie,
		Creeper,
		Dragon
	}
}