using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Blaze : HostileMob
	{
		public Blaze(World level) : base((EntityType)43, level)
		{
			JavaEntityId = 61;
			Height = 1.8;
			Width = 0.6;
		}
	}
}
