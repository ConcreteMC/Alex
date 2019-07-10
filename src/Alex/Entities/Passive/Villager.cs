using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Villager : PassiveMob
	{
		public Villager(World level) : base((EntityType)15, level)
		{
			JavaEntityId = 120;
			Height = 1.95;
			Width = 0.6;
		}
	}
}
