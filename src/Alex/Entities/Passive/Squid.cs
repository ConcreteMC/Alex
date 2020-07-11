using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Squid : WaterMob
	{
		public Squid(World level) : base(EntityType.Squid, level)
		{
			JavaEntityId = 94;
			Height = 0.8;
			Width = 0.8;
		}
	}
}
