using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class VillagerGolem : PassiveMob
	{
		public VillagerGolem(World level) : base(EntityType.IronGolem, level)
		{
			JavaEntityId = 99;
			Height = 2.7;
			Width = 1.4;
		}
	}
}
