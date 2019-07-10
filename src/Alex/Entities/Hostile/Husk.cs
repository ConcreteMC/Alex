using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Husk : HostileMob
	{
		public Husk(World level) : base((EntityType)47, level)
		{
			JavaEntityId = 23;
			Height = 1.95;
			Width = 0.6;
		}
	}
}
