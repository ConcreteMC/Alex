using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Bat : PassiveMob
	{
		public Bat(World level) : base((EntityType)19, level)
		{
			JavaEntityId = 65;
			Height = 0.9;
			Width = 0.5;
		}
	}
}
