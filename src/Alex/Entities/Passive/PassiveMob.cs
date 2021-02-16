using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities.Passive
{
	public abstract class PassiveMob : AgeableEntity
	{
		protected PassiveMob(EntityType type, World level)
			: base(type, level, null)
		{
			
		}
	}
}
