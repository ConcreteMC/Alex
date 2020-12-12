using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Silverfish : HostileMob
	{
		public Silverfish(World level) : base((EntityType)39, level)
		{
			Height = 0.3;
			Width = 0.4;
		}
	}
}
