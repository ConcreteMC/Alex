using Alex.Worlds;

namespace Alex.Entities.Passive
{
	public abstract class PassiveMob : Mob
	{
		protected PassiveMob(EntityType type, World level)
			: base(type, level, null)
		{
			
		}
	}
}
