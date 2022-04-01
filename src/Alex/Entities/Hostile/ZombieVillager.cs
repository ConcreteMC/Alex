using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class ZombieVillager : HostileMob
	{
		public ZombieVillager(World level) : base(level)
		{
			Height = 1.95;
			Width = 0.6;
		}
	}
}