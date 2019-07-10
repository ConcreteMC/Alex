using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Witch : HostileMob
	{
		public Witch(World level) : base((EntityType)45, level)
		{
			JavaEntityId = 66;
			Height = 1.95;
			Width = 0.6;
		}
	}
}
