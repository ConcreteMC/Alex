using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class VindicationIllager : HostileMob
	{
		public VindicationIllager(World level) : base(EntityType.Vindicator, level)
		{
			JavaEntityId = 36;
			Height = 1.95;
			Width = 0.6;
		}
	}
}
