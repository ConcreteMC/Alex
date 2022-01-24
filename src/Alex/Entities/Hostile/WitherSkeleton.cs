using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Hostile
{
	public class WitherSkeleton : HostileMob
	{
		public WitherSkeleton(World level) : base(level)
		{
			Height = 2.4;
			Width = 0.7;
		}
	}
}