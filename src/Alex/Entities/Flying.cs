using Alex.Entities.Hostile;
using Alex.Net;
using Alex.Worlds;
using MiNET.Entities;

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