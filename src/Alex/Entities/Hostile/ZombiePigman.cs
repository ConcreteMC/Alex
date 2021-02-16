using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class ZombiePigman : HostileMob
	{
		public ZombiePigman(World level) : base((EntityType)36, level)
		{
			Height = 1.95;
			Width = 0.6;
		}
	}
}
