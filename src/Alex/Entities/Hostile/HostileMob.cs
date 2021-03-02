using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public abstract class HostileMob : Mob
	{
		protected HostileMob(World level)
			: base(level)
		{
		}
	}
}
