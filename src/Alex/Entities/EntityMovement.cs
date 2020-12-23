using System;
using Alex.API.Utils;
using Alex.API.World;
using Microsoft.Xna.Framework;
using MathF = Alex.API.Utils.MathF;

namespace Alex.Entities
{
	/// <summary>
	/// 	TODO: Implement interpolated movement by seperating KnownPosition from RenderPosition
	/// </summary>
	public class EntityMovement : ITicked
	{
		public Entity  Entity  { get; }
		public Vector3 Heading { get; private set; }

		public bool InterpolatedMovement { get; set; } = true;
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
			var distance = Microsoft.Xna.Framework.Vector3.Distance(
				Entity.KnownPosition.ToVector3() * new Vector3(1f, 0f, 1f), location.ToVector3() * new Vector3(1f, 0f, 1f));
			
			//var difference = 
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
			
			UpdateTarget();

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

			UpdateTarget();
			
			Entity.DistanceMoved +=
				MathF.Abs(Microsoft.Xna.Framework.Vector3.Distance(oldPosition * new Vector3(1f, 0f, 1f), Entity.KnownPosition.ToVector3() * new Vector3(1f, 0f, 1f)));
		}

		private PlayerLocation _from;
		private PlayerLocation _target;
		private void UpdateTarget()
		{
			var target = Entity.KnownPosition;

			if (!InterpolatedMovement)
			{
				Entity.RenderLocation = target;
				return;
			}
			
			var distance = Microsoft.Xna.Framework.Vector3.Distance(
				Entity.RenderLocation.ToVector3() * new Vector3(1f, 0f, 1f), target.ToVector3() * new Vector3(1f, 0f, 1f));

			if (distance > 8f)
			{
				Entity.RenderLocation = target;
				_frameAccumulator = TargetTime;
			}
			else
			{
				_frameAccumulator = 0;
				_from = (PlayerLocation) Entity.RenderLocation.Clone();
				_target = (PlayerLocation) Entity.KnownPosition.Clone();
			}
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

		private       float _frameAccumulator = 0f;
		private const float TargetTime        = 1f / 20f;
		public void Update(GameTime gt)
		{
			var entity    = Entity;

			if ((_target == null || _from == null))
			{
				UpdateTarget();
				return;
			}

			if (_frameAccumulator >= TargetTime)
				return;

			var frameTime = (float) gt.ElapsedGameTime.TotalSeconds; // / 50;
			_frameAccumulator += frameTime;

			var alpha                 = (float) (_frameAccumulator / TargetTime);
			alpha = Math.Min(1f, alpha);
			
			var targetPosition        = _target;
			var previousStatePosition = _from;

			//var pos = Vector3.Lerp(previousStatePosition.ToVector3(), position.ToVector3(), alpha);
			var pos = targetPosition.ToVector3() * alpha + previousStatePosition.ToVector3() * (1f - alpha);

			//var yaw = MathHelper.Lerp(previousStatePosition.Yaw, position.Yaw, alpha);
			var yaw = targetPosition.Yaw * alpha + previousStatePosition.Yaw * (1f - alpha);

			//var headYaw = MathHelper.Lerp(previousStatePosition.HeadYaw, position.HeadYaw, alpha);
			var headYaw = targetPosition.HeadYaw * alpha + previousStatePosition.HeadYaw * (1f - alpha);

			var pitch = targetPosition.Pitch * alpha + previousStatePosition.Pitch * (1f - alpha);
			//var pitch = MathHelper.Lerp(previousStatePosition.Pitch, position.Pitch, alpha);

			var renderLocation = entity.RenderLocation;
			
			renderLocation.X = pos.X;
			renderLocation.Y = pos.Y;
			renderLocation.Z = pos.Z;
			
			renderLocation.HeadYaw = headYaw;
			renderLocation.Yaw = yaw;
			renderLocation.Pitch = pitch;
			
			renderLocation.OnGround = targetPosition.OnGround;

			entity.RenderLocation = renderLocation;
		}

		/// <inheritdoc />
		public void OnTick()
		{
			//UpdateTarget();
		}
	}
}