using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class PolarBear : PassiveMob
	{
		public PolarBear(World level) : base((EntityType)28, level)
		{
			JavaEntityId = 102;
			Height = 1.4;
			Width = 1.3;
		}
	}
}
