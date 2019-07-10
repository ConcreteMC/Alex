using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Chicken : PassiveMob
	{
		public Chicken(World level) : base((EntityType)10, level)
		{
			JavaEntityId = 93;
			Height = 0.7;
			Width = 0.4;
		}
	}
}
