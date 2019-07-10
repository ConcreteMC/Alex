using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class CaveSpider : HostileMob
	{
		public CaveSpider(World level) : base((EntityType)40, level)
		{
			JavaEntityId = 59;
			Height = 0.5;
			Width = 0.7;
		}
	}
}
