using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Shulker : HostileMob
	{
		public Shulker(World level) : base((EntityType)54, level)
		{
			Height = 1;
			Width = 1;
		}
	}
}
