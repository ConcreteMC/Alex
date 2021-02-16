using Alex.Entities.Hostile;
using Alex.Net;
using Alex.Worlds;
using MiNET.Entities;

namespace Alex.Entities
{
	public abstract class Flying : HostileMob
	{
		/// <inheritdoc />
		protected Flying(EntityType type, World level) : base(type, level)
		{
			IsAffectedByGravity = false;
		}
	}
}