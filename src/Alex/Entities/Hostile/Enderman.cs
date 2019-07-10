using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Enderman : HostileMob
	{
		public Enderman(World level) : base((EntityType)38, level)
		{
			JavaEntityId = 58;
			Height = 2.9;
			Width = 0.6;
		}
	}
}
