using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class Vex : HostileMob
	{
		public Vex(World level) : base(level)
		{
			Height = 0.8;
			Width = 0.4;
		}
	}
}
