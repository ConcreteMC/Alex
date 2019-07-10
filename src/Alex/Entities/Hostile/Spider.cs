using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Spider : HostileMob
	{
		public Spider(World level) : base((EntityType)35, level)
		{
			JavaEntityId = 52;
			Height = 0.9;
			Width = 1.4;
		}
	}
}
