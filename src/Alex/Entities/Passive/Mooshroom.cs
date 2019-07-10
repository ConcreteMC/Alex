using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public class Mooshroom : PassiveMob
	{
		public Mooshroom(World level) : base(EntityType.MushroomCow, level)
		{
			JavaEntityId = 96;
			Height = 1.4;
			Width = 0.9;
		}
	}
}
