using Alex.API.Network;
using Alex.Worlds;

namespace Alex.Entities
{
	public class Mob : Entity
	{
		public Mob(int entityTypeId, World level, INetworkProvider network) : base(entityTypeId, level, network)
		{
			Width = Length = 0.6;
			Height = 1.80;
		}

		public Mob(EntityType mobTypes, World level, INetworkProvider network) : this((int)mobTypes, level, network)
		{
		}

		public override void OnTick()
		{
			base.OnTick();
		}
	}
}
