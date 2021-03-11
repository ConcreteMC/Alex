using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Alex.API.Data.Servers;
using Alex.API.Utils;
using Alex.API.Utils.Collections;
using Alex.API.Utils.Vectors;
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
		
		private float GetSlipperiness(Entity entity)
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
		
		private void UpdatePhysics(Entity e)
		{
			if (!e.IsSpawned)
				return;
			
			if (e.NoAi)
				return;
			
			var onGround       = e.KnownPosition.OnGround;
			
			var slipperiness   = 0.91f;
			var movementFactor = (float) e.CalculateMovementSpeed();

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
			
			e.Velocity += ConvertHeading(e, movementFactor);
			//var momentum     = e.Velocity * e.Slipperines * 0.91f;
			//var acceleration = (ConvertMovementIntoVelocity(e, out var slipperiness));

			e.Movement.Move(e.Velocity);


			if (e.IsAffectedByGravity && !e.IsFlying && !e.KnownPosition.OnGround)
			{ 
				e.Velocity -= new Vector3(0f, (float) (e.Gravity), 0f);
			}

			if (e.IsFlying)
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

		    var minX = (int) MathF.Floor(min.X);
		    var maxX = (int) MathF.Ceiling(max.X);

		    var minZ = (int) MathF.Floor(min.Z);
		    var maxZ = (int) MathF.Ceiling(max.Z);

		    var minY = (int) MathF.Floor(min.Y);
		    var maxY = (int) MathF.Ceiling(max.Y);

		    for (int x = minX; x < maxX; x++)
		    for (int y = minY; y < maxY; y++)
		    for (int z = minZ; z < maxZ; z++)
		    {
			    var coords = new BlockCoordinates(new Vector3(x, y, z));

			    var block = world.GetBlockState(coords);

			    if (block == null)
				    continue;

			    if (!block.Block.Solid)
				    continue;

			    foreach (var blockBox in block.Block.GetBoundingBoxes(coords))
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
