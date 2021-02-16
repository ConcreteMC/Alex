using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Passive
{
	public class VillagerGolem : PassiveMob
	{
		public VillagerGolem(World level) : base(EntityType.IronGolem, level)
		{
			Height = 2.7;
			Width = 1.4;
		}
	}
}
