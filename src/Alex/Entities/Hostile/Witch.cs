using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Witch : HostileMob
	{
		public Witch(World level) : base(level)
		{
			Height = 1.95;
			Width = 0.6;
		}
	}
}