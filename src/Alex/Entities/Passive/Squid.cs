using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Squid : PassiveMob
	{
		public Squid(World level) : base((EntityType)17, level)
		{
			JavaEntityId = 94;
			Height = 0.8;
			Width = 0.8;
		}
	}
}
