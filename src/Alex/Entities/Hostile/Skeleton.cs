using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Skeleton : HostileMob
	{
		public Skeleton(World level) : base(level)
		{
			Height = 1.99;
			Width = 0.6;
		}
	}
}