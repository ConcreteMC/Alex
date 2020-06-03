using Alex.API.Network;
using Alex.Net;
using Alex.Worlds;

namespace Alex.Entities
{
	public class Mob : LivingEntity
	{
		public Mob(int entityTypeId, World level, NetworkProvider network) : base(entityTypeId, level, network)
		{
			Width = Length = 0.6;
			Height = 1.80;
		}

		public Mob(EntityType mobTypes, World level, NetworkProvider network) : this((int)mobTypes, level, network)
		{
		}

		public override void OnTick()
		{
			base.OnTick();
		}
	}
}
