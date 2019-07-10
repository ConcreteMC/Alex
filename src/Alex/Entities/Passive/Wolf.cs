using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Wolf : PassiveMob
	{
		public Wolf(World level) : base((EntityType)14, level)
		{
			JavaEntityId = 95;
			Height = 0.85;
			Width = 0.6;
		}
	}
}
