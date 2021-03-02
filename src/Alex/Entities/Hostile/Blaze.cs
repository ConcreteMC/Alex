using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class Blaze : HostileMob
	{
		public Blaze(World level) : base(level)
		{
			Height = 1.8;
			Width = 0.6;
		}
	}
}
