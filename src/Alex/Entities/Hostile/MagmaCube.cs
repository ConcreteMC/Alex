using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class MagmaCube : HostileMob
	{
		public MagmaCube(World level) : base((EntityType)42, level)
		{
			JavaEntityId = 62;
			Height = 0.51000005;
			Width = 0.51000005;
		}
	}
}
