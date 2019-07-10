using Alex.Worlds;

namespace Alex.Entities.Hostile
{
	public class Creeper : HostileMob
	{
		public Creeper(World level) : base((EntityType)33, level)
		{
			JavaEntityId = 50;
			Height = 1.7;
			Width = 0.6;
		}
	}
}
