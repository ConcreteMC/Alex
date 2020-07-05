using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public abstract class HostileMob : Mob
	{
		protected HostileMob(int entityTypeId, World level)
			: base(entityTypeId, level, null)
		{
		}

		protected HostileMob(EntityType type, World level)
			: base(type, level, null)
		{
		}
	}
}
