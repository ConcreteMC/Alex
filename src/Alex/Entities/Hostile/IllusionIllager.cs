using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class IllusionIllager : HostileMob
	{
		public IllusionIllager(World level) : base((EntityType)0, level)
		{
			JavaEntityId = 37;
			Height = 0;
			Width = 0;
		}
	}
}
