using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class Skeleton : HostileMob
	{
		public Skeleton(World level) : base((EntityType)34, level)
		{
			Height = 1.99;
			Width = 0.6;
		}
	}
}
