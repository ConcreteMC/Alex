using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Wither : HostileMob
	{
		public Wither(World level) : base(level)
		{
			Height = 3.5;
			Width = 0.9;
		}
	}
}