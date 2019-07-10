using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Shulker : HostileMob
	{
		public Shulker(World level) : base((EntityType)54, level)
		{
			JavaEntityId = 69;
			Height = 1;
			Width = 1;
		}
	}
}
