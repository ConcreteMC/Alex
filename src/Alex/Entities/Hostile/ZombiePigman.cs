using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class ZombiePigman : HostileMob
	{
		public ZombiePigman(World level) : base((EntityType)36, level)
		{
			JavaEntityId = 57;
			Height = 1.95;
			Width = 0.6;
		}
	}
}
