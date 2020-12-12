using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class EnderDragon : HostileMob
	{
		public EnderDragon(World level) : base(EntityType.EnderDragon, level)
		{
			Height = 8;
			Width = 16;
		}
	}
}
