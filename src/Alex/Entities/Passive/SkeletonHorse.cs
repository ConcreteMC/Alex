using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class SkeletonHorse : PassiveMob
	{
		public SkeletonHorse(World level) : base((EntityType)26, level)
		{
			JavaEntityId = 28;
			Height = 1.6;
			Width = 1.396484;
		}
	}
}
