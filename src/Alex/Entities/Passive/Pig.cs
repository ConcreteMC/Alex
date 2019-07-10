using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Pig : PassiveMob
	{
		public Pig(World level) : base((EntityType)12, level)
		{
			JavaEntityId = 90;
			Height = 0.9;
			Width = 0.9;
		}
	}
}
