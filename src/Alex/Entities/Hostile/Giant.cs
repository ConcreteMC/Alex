using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Giant : HostileMob
	{
		public Giant(World level) : base((EntityType)0, level)
		{
			JavaEntityId = 53;
			Height = 10.8;
			Width = 3.6;
		}
	}
}
