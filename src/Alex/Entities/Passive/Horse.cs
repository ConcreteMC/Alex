using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Horse : PassiveMob
	{
		public Horse(World level) : base((EntityType)23, level)
		{
			JavaEntityId = 100;
			Height = 1.6;
			Width = 1.396484;
		}
	}
}
