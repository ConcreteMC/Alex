using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class WitherSkeleton : HostileMob
	{
		public WitherSkeleton(World level) : base((EntityType)48, level)
		{
			JavaEntityId = 5;
			Height = 2.4;
			Width = 0.7;
		}
	}
}
