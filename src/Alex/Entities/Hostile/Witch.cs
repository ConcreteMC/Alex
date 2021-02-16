using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class Witch : HostileMob
	{
		public Witch(World level) : base((EntityType)45, level)
		{
			Height = 1.95;
			Width = 0.6;
		}
	}
}
