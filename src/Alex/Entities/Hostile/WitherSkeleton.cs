using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class WitherSkeleton : HostileMob
	{
		public WitherSkeleton(World level) : base((EntityType)48, level)
		{
			Height = 2.4;
			Width = 0.7;
		}
	}
}
