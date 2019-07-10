using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Slime : HostileMob
	{
		public Slime(World level) : base((EntityType)37, level)
		{
			JavaEntityId = 55;
			Height = 0.51000005;
			Width = 0.51000005;
		}
	}
}
