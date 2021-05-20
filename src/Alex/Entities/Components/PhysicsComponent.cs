using System;
using Alex.API.Utils.Vectors;
using Microsoft.Xna.Framework;
using NLog;
using MathF = System.MathF;

namespace Alex.Entities.Components
{
	/// <summary>
	///		Handles entity physics
	///		Collision detection heavily based on https://github.com/ddevault/TrueCraft/blob/master/TrueCraft.Core/Physics/PhysicsEngine.cs
	/// </summary>
    public class PhysicsComponent : FixedRateEntityComponent
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(PhysicsComponent));
	    public PhysicsComponent(Entity entity) : base(entity, 20)
	    {
		    
	    }
	    
		//private ThreadSafeList<Entity> PhysicsEntities { get; set; } = new ThreadSafeList<Entity>();
		
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

			var onGround       = e.KnownPosition.OnGround;
			
			var slipperiness   = 0.91f;
			var movementFactor = (float) e.CalculateMovementSpeed();

			if (e.FeetInWater)
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

			e.Velocity += ConvertHeading(e, movementFactor);
			//var momentum     = e.Velocity * e.Slipperines * 0.91f;
			//var acceleration = (ConvertMovementIntoVelocity(e, out var slipperiness));

			e.Movement.Move(e.Velocity);


			if (e.IsAffectedByGravity && !e.IsFlying && !e.KnownPosition.OnGround)
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

		/*public void Update(GameTime elapsed)
		{
			var frameTime = (float) elapsed.ElapsedGameTime.TotalSeconds; // / 50;
			_frameAccumulator += frameTime;

		//	if (_frameAccumulator >= (TargetTime * 1.3))
		//	{
			//	Log.Warn($"Physics running slow! Running {(_frameAccumulator / TargetTime)} ticks behind... (DeltaTime={_frameAccumulator}s Target={TargetTime}s)");
		//	}

			var entities = PhysicsEntities.ToArray();
			//var realTime = entities.Where(x => (x.RequiresRealTimeTick || (x.IsRendered && x.ServerEntity))).ToArray();
			
			while (_frameAccumulator >= TargetTime)
			{
				foreach (var entity in entities)
				{
					try
					{
						UpdatePhysics(entity);
					}
					catch (Exception ex)
					{
						Log.Warn(ex, $"Entity tick threw exception: {ex.ToString()}");
					}
				}
				
				_frameAccumulator -= TargetTime;
			}

			/*var alpha = (float) (_frameAccumulator / TargetTime);

			foreach (var entity in entities)
			{
				UpdateEntityLocation(entity, alpha);
			}*
		}*/

		private float GetSlipperiness(Entity entity)
		{
			var blockcoords = entity.KnownPosition.GetCoordinates3D();

			//if (entity.KnownPosition.Y % 1 <= 0.01f)
			//{
			//	blockcoords = blockcoords.BlockDown();
			//	}
			
			if (entity.BoundingBox.Min.Y % 1 < 0.05f)
			{
				blockcoords -= new BlockCoordinates(0, 1, 0);
			}
				
			var block = Entity?.Level?.GetBlockState(blockcoords.X, blockcoords.Y, blockcoords.Z);
			var slipperiness = (float) block.Block.BlockMaterial.Slipperiness;

			return slipperiness;
		}

		private Vector3 ConvertHeading(Entity entity, float multiplier)
		{
			var heading  = entity.Movement.Heading;
			var strafe   = heading.X;
			var forward  = heading.Z;
			var vertical = entity.IsFlying ? heading.Y : 0f;
			
			var speed    = MathF.Sqrt(strafe * strafe + forward * forward + vertical * vertical);
			if (speed < 0.01f)
				return Vector3.Zero;

			speed = multiplier / MathF.Max(speed, 1f);

			strafe *= speed;
			forward *= speed;
			vertical *= speed;
			

			return new Vector3(strafe, vertical, forward);
		}
		/*public bool AddTickable(Entity entity)
		{
			var collection = PhysicsEntities;

			if (collection == null)
				return false;
			
		    return collection.TryAdd(entity);
	    }

	    public bool Remove(Entity entity)
	    {
		    var collection = PhysicsEntities;

		    if (collection == null)
			    return false;
		    
		    return collection.Remove(entity);
	    }*/

		/// <inheritdoc />
	    public void Dispose()
	    {
		   // var entities = PhysicsEntities.TakeAndClear();
		   // PhysicsEntities = null;
	    }
    }
}
