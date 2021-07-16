using Alex.MoLang.Attributes;
using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class PolarBear : PassiveMob
	{
		[MoProperty("standing_scale")]
		public double StandingScale { get; set; } = 1d;
		public PolarBear(World level) : base(level)
		{
			Height = 1.4;
			Width = 1.3;
		}
	}
}
