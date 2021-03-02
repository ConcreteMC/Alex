using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class Enderman : HostileMob
	{
		public Enderman(World level) : base(level)
		{
			Height = 2.9;
			Width = 0.6;
		}
	}
}
