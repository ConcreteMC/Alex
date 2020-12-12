using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Cow : PassiveMob
	{
		public Cow(World level) : base((EntityType)11, level)
		{
			Height = 1.4;
			Width = 0.9;
		}
	}
}
