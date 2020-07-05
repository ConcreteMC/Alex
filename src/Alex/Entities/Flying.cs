using Alex.Entities.Hostile;
using Alex.Net;
using Alex.Worlds;

namespace Alex.Entities
{
	public abstract class Flying : HostileMob
	{
		/// <inheritdoc />
		protected Flying(EntityType type, World level) : base(type, level) { }
	}
}