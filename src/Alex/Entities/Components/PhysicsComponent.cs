using System;
using Alex.Common.Utils.Vectors;
using Microsoft.Xna.Framework;
using NLog;
using MathF = System.MathF;

namespace Alex.Entities.Components
{
	/// <summary>
	///		Handles entity physics
	/// </summary>
	public class PhysicsComponent : FixedRateEntityComponent
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(PhysicsComponent));

		public PhysicsComponent(Entity entity) : base(entity, "Physics", 20) { }

		private Vector3 TruncateVelocity(Vector3 velocity)
		{
			if (Math.Abs(velocity.X) < 0.005f)
				velocity = new Vector3(0, velocity.Y, velocity.Z);

			if (Math.Abs(velocity.Y) < 0.005f)
				velocity = new Vector3(velocity.X, 0, velocity.Z);

			if (Math.Abs(velocity.Z) < 0.005f)
				velocity = new Vector3(velocity.X, velocity.Y, 0);

			return velocity;
		}

		/// <inheritdoc />
		protected override void OnUpdate(float deltaTime)
		{
			var e = Entity;

			if (!e.IsSpawned || e.NoAi || !Enabled || e.Level == null)
				return;

			var onGround = e.KnownPosition.OnGround;

			var slipperiness = 0.91f;
			var movementFactor = (float)e.CalculateMovementFactor();

			if (e.FeetInWater && !e.IsFlying)
			{
				movementFactor = 0.02f;
				slipperiness = 0.8f;
			}
			else
			{
				if (onGround)
				{
					slipperiness *= GetSlipperiness(e);
					e.Slipperines = slipperiness;

					var acceleration = 0.1627714f / (slipperiness * slipperiness * slipperiness);
					movementFactor *= acceleration;
				}
				else
				{
					if (e.IsFlying)
					{
						movementFactor *= 0.1627714f / (slipperiness * slipperiness * slipperiness);
					}
					else
					{
						movementFactor = 0.02f;
					}
				}
			}

			var heading = e.Movement.Heading;

			if (e.IsSwimming)
			{
				heading = heading.Transform(Matrix.CreateRotationX(e.KnownPosition.Pitch.ToRadians()));
			}

			var strafing = heading.X * 0.98F;
			var forward = heading.Z * 0.98F;

			if (e.IsSneaking)
			{
				strafing *= 0.3f;
				forward *= 0.3f;
			}

			heading = ConvertHeading(
				e.KnownPosition.HeadYaw, strafing, forward, (e.IsFlying || e.IsSwimming) ? heading.Y : 0f,
				movementFactor);

			e.Velocity += heading;

			//var momentum     = e.Velocity * e.Slipperines * 0.91f;
			//var acceleration = (ConvertMovementIntoVelocity(e, out var slipperiness));

			e.Movement.Move(TruncateVelocity(e.Velocity));


			if (e.IsAffectedByGravity && !e.IsFlying && !e.KnownPosition.OnGround && !e.IsSwimming)
			{
				var gravity = (float)e.Gravity;

				if (e.FeetInWater)
					gravity /= 4f;

				e.Velocity -= new Vector3(0f, gravity, 0f);
			}

			if (e.IsFlying || e.FeetInWater)
			{
				e.Velocity *= new Vector3(slipperiness, slipperiness, slipperiness);
			}
			else
			{
				//e.Velocity -= new Vector3(0f, (float) gravity, 0f);
				e.Velocity *= new Vector3(slipperiness, 0.98f, slipperiness);
			}

			e.Velocity = TruncateVelocity(e.Velocity);
		}

		private float GetSlipperiness(Entity entity)
		{
			var blockcoords = entity.KnownPosition.GetCoordinates3D();

			if (entity.BoundingBox.Min.Y % 1 < 0.05f)
			{
				blockcoords -= new BlockCoordinates(0, 1, 0);
			}

			var block = Entity?.Level?.GetBlockState(blockcoords.X, blockcoords.Y, blockcoords.Z);
			var slipperiness = (float)block.Block.BlockMaterial.Slipperiness;

			return slipperiness;
		}

		private Vector3 ConvertHeading(float yaw, float strafe, float forward, float vertical, float multiplier)
		{
			var speed = MathF.Sqrt(strafe * strafe + forward * forward + vertical * vertical);

			if (speed < 0.01f)
				return Vector3.Zero;

			speed = multiplier / MathF.Max(speed, 1f);

			strafe *= speed;
			forward *= speed;
			vertical *= speed;

			var rotationYaw = -yaw;
			var sinYaw = MathF.Sin(rotationYaw * MathF.PI / 180.0F);
			var cosYaw = MathF.Cos(rotationYaw * MathF.PI / 180.0F);

			return new Vector3(strafe * cosYaw - forward * sinYaw, vertical, forward * cosYaw + strafe * sinYaw);
		}
	}
}