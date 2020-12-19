using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Entities
{
	/// <summary>
	/// 	TODO: Implement interpolated movement by seperating KnownPosition from RenderPosition
	/// </summary>
	public class EntityMovement
	{
		public Entity  Entity  { get; }
		public Vector3 Heading { get; private set; }
		
		public EntityMovement(Entity entity)
		{
			Entity = entity;
			Heading = Vector3.Zero;
		}

		private object _headingLock = new object();
		public void UpdateHeading(Vector3 heading)
		{
			lock (_headingLock)
			{
				Heading = Vector3.Transform(heading, Matrix.CreateRotationY(-MathHelper.ToRadians(Entity.KnownPosition.HeadYaw)));;
			}
		}
		
		public void MoveTo(PlayerLocation location, bool updateLook = true)
		{
			//var difference = 
			var distance = Microsoft.Xna.Framework.Vector3.Distance(
				Entity.KnownPosition.ToVector3(), location.ToVector3());

			Entity.KnownPosition = location;
			
			//Entity.KnownPosition.X = location.X;
			//Entity.KnownPosition.Y = location.Y;
			//Entity.KnownPosition.Z = location.Z;
			Entity.KnownPosition.OnGround = location.OnGround;

			if (updateLook)
			{
				Entity.KnownPosition.Yaw = location.Yaw;
				Entity.KnownPosition.HeadYaw = location.HeadYaw;
				Entity.KnownPosition.Pitch = location.Pitch;
			}

			Entity.DistanceMoved += MathF.Abs(distance);
		}

		public void Move(Vector3 direction)
		{
			var   oldPosition = Entity.KnownPosition.ToVector3();
			float offset      = 0f;

			//TODO: Fix position offset

			Entity.KnownPosition.X += direction.X;
			Entity.KnownPosition.Y += (direction.Y - offset);
			Entity.KnownPosition.Z += direction.Z;

			Entity.DistanceMoved +=
				MathF.Abs(Microsoft.Xna.Framework.Vector3.Distance(oldPosition, Entity.KnownPosition.ToVector3()));
		}

		public void Push(Vector3 velocity)
		{
			Entity.Velocity += velocity;
		}

		public void Velocity(Vector3 velocity)
		{
			var oldLength = (Entity.Velocity).Length();
			if (oldLength < velocity.Length())
			{
				Entity.Velocity += new Vector3(velocity.X - Entity.Velocity.X, velocity.Y - Entity.Velocity.Y, velocity.Z - Entity.Velocity.Z);
			}
			else
			{
				Entity.Velocity = new Vector3(
					MathF.Abs(Entity.Velocity.X) < 0.0001f ? velocity.Y : Entity.Velocity.X,
					MathF.Abs(Entity.Velocity.Y) < 0.0001f ? velocity.Y : Entity.Velocity.Y,
					MathF.Abs(Entity.Velocity.Z) < 0.0001f ? velocity.Z : Entity.Velocity.Z);
			}
		}
		
		public void Teleport(PlayerLocation location)
		{
			Entity.KnownPosition = location;
			/*
			var oldPosition = entity.KnownPosition;
			entity.KnownPosition = position;
			//if (entity is PlayerMob p)
			{
				entity.DistanceMoved += MathF.Abs(Vector3.Distance(oldPosition, position));
			}*/
		}
	}
}