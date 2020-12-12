using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Giant : HostileMob
	{
		public Giant(World level) : base((EntityType)0, level)
		{
			Height = 10.8;
			Width = 3.6;
		}
	}
}
