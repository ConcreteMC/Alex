using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Zombie : HostileMob
	{
		public Zombie(World level) : base(level)
		{
			Height = 1.95;
			Width = 0.6;
		}
	}
}