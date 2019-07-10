using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Endermite : HostileMob
	{
		public Endermite(World level) : base((EntityType)55, level)
		{
			JavaEntityId = 67;
			Height = 0.3;
			Width = 0.4;
		}
	}
}
