using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class EnderDragon : HostileMob
	{
		public EnderDragon(World level) : base(EntityType.EnderDragon, level)
		{
			JavaEntityId = 63;
			Height = 8;
			Width = 16;
		}
	}
}
