using Alex.Net;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities.Projectiles
{
	public class FireworkRocket : ThrowableEntity
	{
		/// <inheritdoc />
		public FireworkRocket(World level) : base(level)
		{
			Width = 0.25;
			Height = 0.25;
			
			Gravity = 0.0;
			Drag = 0.01;
			
			HasCollision = true;
			IsAffectedByGravity = true;
			StopOnImpact = false;
		}

		/// <inheritdoc />
		public override void HandleEntityEvent(byte eventId, int data)
		{
			if (eventId == 25)
			{
				
				return;
			}
			base.HandleEntityEvent(eventId, data);
		}

		/// <inheritdoc />
		public override float CollidedWithWorld(Vector3 direction, Vector3 position, float impactVelocity)
		{
			if (StopOnImpact)
			{
				if (direction == Vector3.Up)
				{
					Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);

					return 0f;
				}
			}

			return base.CollidedWithWorld(direction, position, impactVelocity);
		}

		/// <inheritdoc />
		public override void OnTick()
		{
			base.OnTick();
		}
	}
}