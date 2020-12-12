using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Vex : HostileMob
	{
		public Vex(World level) : base((EntityType)105, level)
		{
			Height = 0.8;
			Width = 0.4;
		}
	}
}
