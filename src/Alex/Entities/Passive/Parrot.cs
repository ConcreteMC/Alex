using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Parrot : PassiveMob
	{
		public Parrot(World level) : base((EntityType)0, level)
		{
			JavaEntityId = 105;
			Height = 0.9;
			Width = 0.5;
		}
	}
}
