using Alex.Net;
using Alex.Worlds;

namespace Alex.Entities
{
	public class ThrowableEntity : Entity
	{
		/// <inheritdoc />
		public ThrowableEntity(int entityTypeId, World level, NetworkProvider network) : base(
			entityTypeId, level, network)
		{
			
		}
	}
}