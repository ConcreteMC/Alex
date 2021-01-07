using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Alex.API.Data.Servers;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Entities;
using Alex.Graphics.Models.Blocks;
using Alex.Utils;
using Microsoft.Xna.Framework;
using NLog;
using MathF = System.MathF;

namespace Alex.Worlds
{
	/// <summary>
	///		Handles entity physics
	///		Collision detection heavily based on https://github.com/ddevault/TrueCraft/blob/master/TrueCraft.Core/Physics/PhysicsEngine.cs
	/// </summary>
    public class PhysicsManager : ITicked
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(PhysicsManager));
	    private World World { get; }

	    public PhysicsManager(World world)
	    {
		    World = world;
	    }
	    
		private ThreadSafeList<Entity> PhysicsEntities { get; } = new ThreadSafeList<Entity>();
		
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
		
		private       float _frameAccumulator = 0f;
		private const float TargetTime        = 1f / 20f;
		public void Update(GameTime elapsed)
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
			}*/
		}
		
		
		/// <inheritdoc />
		public void OnTick()
		{
			return;
		}

		//private void Apply

		private Vector3 ConvertMovementIntoVelocity(Entity entity, out float slipperiness)
		{
			slipperiness = 0.6f;
			
			var movement = entity.Movement.Heading * 0.98F;
			//movement.Y = 0f;

			float mag = movement.LengthSquared();//movement.X * movement.X + movement.Z * movement.Z;
			// don't do insignificant movement
			if (mag < 0.01f) {
				return Vector3.Zero;
			}

			//movement.X /= mag;
			//movement.Z /= mag;

			//mag *=  (float)entity.CalculateMovementSpeed();
			//movement *=mag;
			
			if (!entity.KnownPosition.OnGround || entity.IsInWater)
			{
				movement *= 0.02f;
			}
			else
			{
				var blockcoords = entity.KnownPosition.GetCoordinates3D();

				//if (entity.KnownPosition.Y % 1 <= 0.01f)
				//{
				//	blockcoords = blockcoords.BlockDown();
			//	}
			
				if (entity.BoundingBox.Min.Y % 1 < 0.05f)
				{
					blockcoords.Y -= 1;
				}
				
				var block = World.GetBlockState(blockcoords.X, blockcoords.Y, blockcoords.Z);
				slipperiness = (float) block.Block.BlockMaterial.Slipperiness;
					
				movement *= (float)entity.CalculateMovementSpeed() * (0.1627714f / (slipperiness * slipperiness * slipperiness));
			}

			return movement;
		}

		private void UpdatePhysics(Entity e)
		{
			if (!e.IsSpawned)
				return;
			//var velocityBeforeAdjustment = new Vector3(e.Velocity.X, e.Velocity.Y, e.Velocity.Z);

			e.Velocity += (ConvertMovementIntoVelocity(e, out var slipperiness));

			//if (e.HasCollision)
			{
				//e.Velocity = TruncateVelocity(e.Velocity);
				
			}
			
			e.Velocity = TruncateVelocity(e.Velocity);
			
			//TestTerrainCollisionCylinder(e, out var collision);
			//	aabbEntity.TerrainCollision(collision, before);

			e.Movement.Move(e.Velocity);
			//e.Movement.MoveTo(e.KnownPosition + e.Velocity);
			
			//e.KnownPosition.OnGround = DetectOnGround(e);

			if (e.IsNoAi)
				return;

			var gravity = e.Gravity;

			if (!e.IsAffectedByGravity)
				gravity = 0;

			if (e.IsFlying && e is Player)
			{
				e.Velocity *= new Vector3(0.9f, 0.9f, 0.9f);
			}
			else
			{
				if (e.IsInWater)
				{
					e.Velocity = new Vector3(
						e.Velocity.X * 0.8f, (float) (e.Velocity.Y - gravity), e.Velocity.Z * 0.8f); //Liquid Drag
				}
				else if (e.IsInLava)
				{
					e.Velocity = new Vector3(
						e.Velocity.X * 0.5f, (float) (e.Velocity.Y - gravity), e.Velocity.Z * 0.5f); //Liquid Drag
				}
				else
				{
					if (e.KnownPosition.OnGround)
					{
						e.Velocity *= new Vector3(slipperiness, 1f, slipperiness);
					}
					else
					{
						if (e.IsAffectedByGravity && !e.IsFlying)
						{
							e.Velocity -= new Vector3(0f, (float) (gravity), 0f);
						}

						e.Velocity *= new Vector3(0.91f, 0.98f, 0.91f);
					}
				}
			}

			//e.Velocity = TruncateVelocity(e.Velocity);
		}

		public bool AddTickable(Entity entity)
	    {
		    return PhysicsEntities.TryAdd(entity);
	    }

	    public bool Remove(Entity entity)
	    {
		    return PhysicsEntities.Remove(entity);
	    }


	    public static IEnumerable<BoundingBox> GetIntersecting(World world, BoundingBox box)
	    {
		    var min = box.Min;
		    var max = box.Max;

		    var minX = (int) Math.Floor(min.X);
		    var maxX = (int) Math.Ceiling(max.X);

		    var minZ = (int) Math.Floor(min.Z);
		    var maxZ = (int) Math.Ceiling(max.Z);

		    var minY = (int) Math.Floor(min.Y);
		    var maxY = (int) Math.Ceiling(max.Y);

		    for (int x = minX; x < maxX; x++)
		    for (int y = minY; y < maxY; y++)
		    for (int z = minZ; z < maxZ; z++)
		    {
			    var coords = new BlockCoordinates(new Vector3(x, y, z));

			    var block = world.GetBlockState(coords);

			    if (block == null || block.Model == null)
				    continue;

			    if (!block.Block.Solid)
				    continue;

			    foreach (var blockBox in block.Model.GetBoundingBoxes(coords))
			    {
				    if (box.Intersects(blockBox))
				    {
					    yield return blockBox;
				    }
			    }
		    }
	    }
    }
}
