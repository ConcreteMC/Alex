using Alex.API.Network;
using Alex.Net;
using Alex.Worlds;

namespace Alex.Entities
{
	public class LivingEntity : Entity
	{
		/// <inheritdoc />
		public LivingEntity(int entityTypeId, World level, NetworkProvider network) : base(
			entityTypeId, level, network)
		{
			
		}
	}
}
