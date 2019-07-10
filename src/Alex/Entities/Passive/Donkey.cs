using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Donkey : PassiveMob
	{
		public Donkey(World level) : base((EntityType)24, level)
		{
			JavaEntityId = 31;
			Height = 1.6;
			Width = 1.396484;
		}
	}
}
