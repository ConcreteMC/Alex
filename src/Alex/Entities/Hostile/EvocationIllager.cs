using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class EvocationIllager : HostileMob
	{
		public EvocationIllager(World level) : base(EntityType.Evoker, level)
		{
			JavaEntityId = 34;
			Height = 1.95;
			Width = 0.6;
		}
	}
}
