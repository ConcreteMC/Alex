using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class Guardian : HostileMob
	{
		public Guardian(World level) : base((EntityType)49, level)
		{
			Height = 0.85;
			Width = 0.85;
		}
	}
}
