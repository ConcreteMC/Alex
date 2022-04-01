using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Stray : HostileMob
	{
		public Stray(World level) : base(level)
		{
			Height = 1.99;
			Width = 0.6;
		}
	}
}