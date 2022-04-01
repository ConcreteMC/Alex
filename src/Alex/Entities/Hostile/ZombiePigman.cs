using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class ZombiePigman : HostileMob
	{
		public ZombiePigman(World level) : base(level)
		{
			Height = 1.95;
			Width = 0.6;
		}
	}
}