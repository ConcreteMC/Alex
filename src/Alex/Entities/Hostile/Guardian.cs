using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Guardian : HostileMob
	{
		public Guardian(World level) : base((EntityType)49, level)
		{
			JavaEntityId = 68;
			Height = 0.85;
			Width = 0.85;
		}
	}
}
