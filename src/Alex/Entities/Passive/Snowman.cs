using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Snowman : PassiveMob
	{
		public Snowman(World level) : base(level)
		{
			Height = 1.9;
			Width = 0.7;
		}
	}
}