using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Zombie : HostileMob
	{
		public Zombie(World level) : base((EntityType)32, level)
		{
			JavaEntityId = 54;
			Height = 1.95;
			Width = 0.6;
		}
	}
}
