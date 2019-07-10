using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Snowman : PassiveMob
	{
		public Snowman(World level) : base(EntityType.SnowGolem, level)
		{
			JavaEntityId = 97;
			Height = 1.9;
			Width = 0.7;
		}
	}
}
