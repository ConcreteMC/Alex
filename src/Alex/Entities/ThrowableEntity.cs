using Alex.Net;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities
{
	public class ThrowableEntity : Entity
	{
		public bool StopOnImpact { get; set; } = false;
		public bool DespawnOnImpact { get; set; } = false;
		
		/// <inheritdoc />
		public ThrowableEntity(int entityTypeId, World level, NetworkProvider network) : base(
			entityTypeId, level, network)
		{
			
		}
		
		/// <inheritdoc />
		public override void CollidedWithWorld(Vector3 direction, Vector3 position)
		{
			if (StopOnImpact)
			{
				Velocity = Vector3.Zero;
				NoAi = true;
			}
		}
	}
}