using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class ElderGuardian : HostileMob
	{
		public ElderGuardian(World level) : base((EntityType)50, level)
		{
			Height = 1.9975;
			Width = 1.9975;
		}
	}
}
