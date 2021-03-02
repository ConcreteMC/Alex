using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class EnderDragon : HostileMob
	{
		public EnderDragon(World level) : base(level)
		{
			Height = 8;
			Width = 16;
		}
	}
}
