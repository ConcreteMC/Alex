using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Cow : PassiveMob
	{
		public Cow(World level) : base(level)
		{
			Height = 1.4;
			Width = 0.9;
		}
	}
}
