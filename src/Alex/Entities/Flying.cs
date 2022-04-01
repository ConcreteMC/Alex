using Alex.Entities.Hostile;
using Alex.Worlds;

namespace Alex.Entities
{
	public abstract class Flying : HostileMob
	{
		/// <inheritdoc />
		protected Flying(World level) : base(level)
		{
			IsAffectedByGravity = false;
		}
	}
}