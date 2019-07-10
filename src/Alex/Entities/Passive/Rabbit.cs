using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Rabbit : PassiveMob
	{
		public Rabbit(World level) : base((EntityType)18, level)
		{
			JavaEntityId = 101;
			Height = 0.5;
			Width = 0.4;
		}
	}
}
