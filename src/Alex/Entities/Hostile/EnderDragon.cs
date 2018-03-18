using Alex.Utils;
using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class EnderDragon : HostileMob
	{
		public EnderDragon(World level) : base(EntityType.Dragon, level)
		{
			JavaEntityId = 63;
			Height = 8;
			Width = 16;
		}
	}
}
