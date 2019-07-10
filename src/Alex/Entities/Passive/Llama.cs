using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Llama : PassiveMob
	{
		public Llama(World level) : base((EntityType)29, level)
		{
			JavaEntityId = 103;
			Height = 1.87;
			Width = 0.9;
		}
	}
}
