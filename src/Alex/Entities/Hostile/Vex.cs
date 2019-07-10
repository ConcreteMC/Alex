using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Vex : HostileMob
	{
		public Vex(World level) : base((EntityType)105, level)
		{
			JavaEntityId = 35;
			Height = 0.8;
			Width = 0.4;
		}
	}
}
