using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class Guardian : HostileMob
	{
		public Guardian(World level) : base(level)
		{
			Height = 0.85;
			Width = 0.85;
		}
	}
}
