using Alex.API.Network;
using Alex.Worlds;

namespace Alex.Entities
{
	public class LivingEntity : Entity
	{
		/// <inheritdoc />
		public LivingEntity(int entityTypeId, World level, INetworkProvider network) : base(
			entityTypeId, level, network)
		{
			
		}
	}
}
