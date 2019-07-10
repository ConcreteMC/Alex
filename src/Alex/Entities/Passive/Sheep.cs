using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Sheep : PassiveMob
	{
		public Sheep(World level) : base((EntityType)13, level)
		{
			JavaEntityId = 91;
			Height = 1.3;
			Width = 0.9;
		}
	}
}
