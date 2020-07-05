using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class ZombieHorse : AbstractHorse
	{
		public ZombieHorse(World level) : base((EntityType)27, level)
		{
			JavaEntityId = 29;
			Height = 1.6;
			Width = 1.396484;
		}
	}
}
