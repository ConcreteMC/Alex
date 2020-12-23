using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Alex.API.Data.Servers;
using Alex.API.Entities;
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
			if (Math.Abs(velocity.X) < 0.0005f)
				velocity = new Vector3(0, velocity.Y, velocity.Z);
			
			if (Math.Abs(velocity.Y) < 0.0005f)
				velocity = new Vector3(velocity.X, 0, velocity.Z);
			
			if (Math.Abs(velocity.Z) < 0.0005f)
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

			var alpha = (float) (_frameAccumulator / TargetTime);

			foreach (var entity in entities)
			{
				UpdateEntityLocation(entity, alpha);
			}
		}
		
		
		/// <inheritdoc />
		public void OnTick()
		{
			return;
			var entities = PhysicsEntities.ToArray();
			var realTime = entities.Where(x => (!x.RequiresRealTimeTick || (!x.IsRendered && x.ServerEntity))).ToArray();
			
			foreach (var entity in realTime)
			{
				UpdatePhysics(entity);
				UpdateEntityLocation(entity, TargetTime);
			}
		}

		private void UpdateEntityLocation(Entity entity, float alpha)
		{
			var position              = entity.KnownPosition;
			var previousStatePosition = entity.PreviousState.Position;

			//var pos = Vector3.Lerp(previousStatePosition.ToVector3(), position.ToVector3(), alpha);
			var pos = position.ToVector3() * alpha + previousStatePosition.ToVector3() * (1f - alpha);

			//var yaw = MathHelper.Lerp(previousStatePosition.Yaw, position.Yaw, alpha);
			var yaw = position.Yaw * alpha + previousStatePosition.Yaw * (1f - alpha);

			//var headYaw = MathHelper.Lerp(previousStatePosition.HeadYaw, position.HeadYaw, alpha);
			var headYaw = position.HeadYaw * alpha + previousStatePosition.HeadYaw * (1f - alpha);

			var pitch = position.Pitch * alpha + previousStatePosition.Pitch * (1f - alpha);
			//var pitch = MathHelper.Lerp(previousStatePosition.Pitch, position.Pitch, alpha);

			var renderLocation = entity.RenderLocation;
			renderLocation.X = pos.X;
			renderLocation.Y = pos.Y;
			renderLocation.Z = pos.Z;
			renderLocation.HeadYaw = headYaw;
			renderLocation.Yaw = yaw;
			renderLocation.Pitch = pitch;
			renderLocation.OnGround = position.OnGround;

			entity.RenderLocation = renderLocation;
		}
		
		//private void Apply

		private Vector3 ConvertMovementIntoVelocity(Entity entity, out float slipperiness)
		{
			slipperiness = 0.6f;
			
			var movement = entity.Movement.Heading * 0.98F;
			movement.Y = 0f;

			float mag = movement.X * movement.X + movement.Z * movement.Z;
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
				
				var block = World.GetBlock(blockcoords.X, blockcoords.Y, blockcoords.Z);
				slipperiness = (float) block.BlockMaterial.Slipperiness;
					
				movement *= (float)entity.CalculateMovementSpeed() * (0.1627714f / (slipperiness * slipperiness * slipperiness));
			}

			return movement;
		}

		private void UpdatePhysics(Entity e)
		{
			List<BoundingBox> boxes = new List<BoundingBox>();
			
			e.PreviousState.Position = (PlayerLocation)e.KnownPosition.Clone();
			var velocityBeforeAdjustment = new Vector3(e.Velocity.X, e.Velocity.Y, e.Velocity.Z);

			e.Velocity += (ConvertMovementIntoVelocity(e, out var slipperiness));

			if (e.HasCollision)
			{
				e.Velocity = TruncateVelocity(e.Velocity);
				
				if (TestTerrainCollisionY(e, out var yCollisionPoint, out var yBox))
				{
					e.CollidedWithWorld(
						velocityBeforeAdjustment.Y < 0 ? Vector3.Down : Vector3.Up, yCollisionPoint,
						velocityBeforeAdjustment.Y);

					boxes.Add(yBox);
				}

				if (TestTerrainCollisionX(e, out var xCollisionPoint, out var xBox))
				{
					e.CollidedWithWorld(
						velocityBeforeAdjustment.X < 0 ? Vector3.Left : Vector3.Right, xCollisionPoint,
						velocityBeforeAdjustment.X);

					boxes.Add(xBox);
				}

				if (TestTerrainCollisionZ(e, out var zCollisionPoint, out var zBox))
				{
					e.CollidedWithWorld(
						velocityBeforeAdjustment.Z < 0 ? Vector3.Backward : Vector3.Forward, zCollisionPoint,
						velocityBeforeAdjustment.Z);

					boxes.Add(zBox);
				}
			}

			//TestTerrainCollisionCylinder(e, out var collision);
			//	aabbEntity.TerrainCollision(collision, before);

			e.Movement.MoveTo(e.KnownPosition + e.Velocity);
			
			e.KnownPosition.OnGround = DetectOnGround(e);

			if (e is Player && boxes.Count > 0)
			{
				LastKnownHit = boxes.ToArray();
			}

			if (e.IsNoAi)
				return;
			
			if (e.IsInWater)
			{
				e.Velocity = new Vector3(e.Velocity.X * 0.8f, (float) (e.Velocity.Y - e.Gravity), e.Velocity.Z * 0.8f); //Liquid Drag
			}
			else if (e.IsInLava)
			{
				e.Velocity = new Vector3(e.Velocity.X * 0.5f, (float) (e.Velocity.Y - e.Gravity), e.Velocity.Z * 0.5f); //Liquid Drag
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
						e.Velocity -= new Vector3(0f, (float) (e.Gravity), 0f);
					}
					
					e.Velocity *= new Vector3(0.91f, 0.98f, 0.91f);
				}
			}
			
			e.Velocity = TruncateVelocity(e.Velocity);
		}

		private BoundingBox GetAABBVelocityBox(Entity entity)
		{
			var min = new Vector3(
				Math.Min(entity.BoundingBox.Min.X, entity.BoundingBox.Min.X + entity.Velocity.X),
				Math.Min(entity.BoundingBox.Min.Y, entity.BoundingBox.Min.Y + entity.Velocity.Y),
				Math.Min(entity.BoundingBox.Min.Z, entity.BoundingBox.Min.Z + entity.Velocity.Z)
			);
			var max = new Vector3(
				Math.Max(entity.BoundingBox.Max.X, entity.BoundingBox.Max.X + entity.Velocity.X),
				Math.Max(entity.BoundingBox.Max.Y, entity.BoundingBox.Max.Y + entity.Velocity.Y),
				Math.Max(entity.BoundingBox.Max.Z, entity.BoundingBox.Max.Z + entity.Velocity.Z)
			);
			return new BoundingBox(min, max);
		}

		
		private void AdjustVelocityForCollision(Entity entity, BoundingBox problem)
		{
			var   velocity = entity.Velocity;

			if (entity.Velocity.X < 0)
				velocity.X = -(entity.BoundingBox.Min.X - problem.Max.X);
			if (entity.Velocity.X > 0)
				velocity.X = entity.BoundingBox.Max.X - problem.Min.X;
			
			if (entity.Velocity.Y < 0)
				velocity.Y = -(entity.BoundingBox.Min.Y - problem.Max.Y);
			if (entity.Velocity.Y > 0)
				velocity.Y = entity.BoundingBox.Max.Y - problem.Min.Y;
			
			if (entity.Velocity.Z < 0)
				velocity.Z = -(entity.BoundingBox.Min.Z - problem.Max.Z);
			if (entity.Velocity.Z > 0)
				velocity.Z = entity.BoundingBox.Max.Z - problem.Min.Z;
			
			entity.Velocity = velocity;
		}
		
		public bool TestTerrainCollisionCylinder(Entity entity, out Vector3 collisionPoint)
		{
			collisionPoint = Vector3.Zero;
			var testBox = GetAABBVelocityBox(entity);
			var testCylinder = new BoundingCylinder(testBox.Min, testBox.Max,
				 Vector3.Distance(entity.BoundingBox.Min, entity.BoundingBox.Max));

			bool collision = false;
			for (int x = (int)(Math.Floor(testBox.Min.X)); x <= (int)(Math.Ceiling(testBox.Max.X)); x++)
			{
				for (int z = (int)(Math.Floor(testBox.Min.Z)); z <= (int)(Math.Ceiling(testBox.Max.Z)); z++)
				{
					for (int y = (int)(Math.Floor(testBox.Min.Y)); y <= (int)(Math.Ceiling(testBox.Max.Y)); y++)
					{
						var blockState = entity.Level.GetBlockState(x, y, z);
						if (blockState?.Model == null || !blockState.Block.Solid)
							continue;

						var coords = new Vector3(x, y, z);

						foreach (var box in blockState.Model.GetBoundingBoxes(coords))
						{
							if (testCylinder.Intersects(box))
							{
								if (testBox.Intersects(box))
								{
									collision = true;
									AdjustVelocityForCollision(entity, box);
									testBox = GetAABBVelocityBox(entity);

									testCylinder = new BoundingCylinder(
										testBox.Min, testBox.Max,
										Vector3.Distance(entity.BoundingBox.Min, entity.BoundingBox.Max));

									collisionPoint = coords;
								}
							}
						}
					}
				}
			}
			return collision;
		}

		private bool TestTerrainCollisionY(Entity entity, out Vector3 collisionPoint, out BoundingBox blockBox)
		{
			collisionPoint = Vector3.Zero;
			blockBox = new BoundingBox();
			
			if (entity.Velocity.Y == 0)
				return false;

			bool negative;

			BoundingBox testBox;
		//	var         entityBox = entity.BoundingBox;

			if (entity.Velocity.Y < 0)
			{
				testBox = new BoundingBox(
					new Vector3(
						entity.BoundingBox.Min.X,  entity.BoundingBox.Min.Y + entity.Velocity.Y,
						entity.BoundingBox.Min.Z),  entity.BoundingBox.Max);

				negative = true;
			}
			else
			{
				testBox = new BoundingBox(
					entity.BoundingBox.Min,
					new Vector3(
						entity.BoundingBox.Max.X,  entity.BoundingBox.Max.Y + entity.Velocity.Y,
						entity.BoundingBox.Max.Z));

				negative = false;
			}

			float? collisionExtent = null;

			for (int x = (int) (Math.Floor(testBox.Min.X)); x <= (int) (Math.Ceiling(testBox.Max.X)); x++)
			{
				for (int z = (int) (Math.Floor(testBox.Min.Z)); z <= (int) (Math.Ceiling(testBox.Max.Z)); z++)
				{
					for (int y = (int) (Math.Floor(testBox.Min.Y)); y <= (int) (Math.Ceiling(testBox.Max.Y)); y++)
					{
						var blockState = entity.Level.GetBlockState(x, y, z);
						if (blockState?.Model == null || !blockState.Block.Solid)
							continue;

						var coords = new Vector3(x, y, z);
						
						foreach (var box in blockState.Model.GetBoundingBoxes(coords))
						{
							if (negative)
							{
								if (entity.BoundingBox.Min.Y - box.Max.Y < 0)
									continue;
							}
							else
							{
								if (box.Min.Y - entity.BoundingBox.Max.Y < 0)
									continue;
							}
							
							if (testBox.Intersects(box))
							{
								if (negative)
								{
									if ((collisionExtent == null || collisionExtent.Value < box.Max.Y))
									{
										collisionExtent = box.Max.Y;
										collisionPoint = coords;
										blockBox = box;
									}
								}
								else
								{
									if ((collisionExtent == null || collisionExtent.Value > box.Min.Y))
									{
										collisionExtent = box.Min.Y;
										collisionPoint = coords;
										blockBox = box;
									}
								}
							}
						}
					}
				}
			}

			if (collisionExtent != null) // Collision detected, adjust accordingly
			{
				var    extent = collisionExtent.Value;
				
				/*if (!negative && CanClimb(entity.Velocity, testBox, blockBox) && entity.KnownPosition.OnGround)
				{
					var yDifference = blockBox.Max.Y - entity.BoundingBox.Min.Y;

					if (yDifference > 0f)
					{
						entity.Velocity = new Vector3(entity.Velocity.X, MathF.Sqrt(2f * (float) (entity.Gravity) * (yDifference)), entity.Velocity.Z);

						return false;
					}
				}*/
				
				float diff;
				if (negative)
					diff = -( entity.BoundingBox.Min.Y - extent);
				else
					diff = extent -  entity.BoundingBox.Max.Y;
				entity.Velocity = new Vector3(entity.Velocity.X, diff, entity.Velocity.Z);
				return true;
			}
			
			return false;
		}

		private bool TestTerrainCollisionX(Entity entity, out Vector3 collisionPoint, out BoundingBox blockBox)
		{
			collisionPoint = Vector3.Zero;
			blockBox = new BoundingBox();

			if (entity.Velocity.X == 0)
				return false;
			
			bool negative;

			BoundingBox testBox;

			if (entity.Velocity.X < 0)
			{
				testBox = new BoundingBox(
					new Vector3(
						entity.BoundingBox.Min.X + entity.Velocity.X, 
						entity.BoundingBox.Min.Y,
						entity.BoundingBox.Min.Z),
					entity.BoundingBox.Max);

				negative = true;
			}
			else
			{
				testBox = new BoundingBox(
					entity.BoundingBox.Min,
					new Vector3(
						entity.BoundingBox.Max.X + entity.Velocity.X, 
						entity.BoundingBox.Max.Y,
						entity.BoundingBox.Max.Z));

				negative = false;
			}

			float? collisionExtent = null;
			
			bool   climable        = true;
			for (int x = (int) (Math.Floor(testBox.Min.X)); x <= (int) (Math.Ceiling(testBox.Max.X)); x++)
			{
				for (int z = (int) (Math.Floor(testBox.Min.Z)); z <= (int) (Math.Ceiling(testBox.Max.Z)); z++)
				{
					for (int y = (int) (Math.Floor(testBox.Min.Y)); y <= (int) (Math.Ceiling(testBox.Max.Y)); y++)
					{
						var blockState = entity.Level.GetBlockState(x, y, z);
						if (blockState?.Model == null || !blockState.Block.Solid)
							continue;

						var coords = new Vector3(x, y, z);
						
						foreach (var box in blockState.Model.GetBoundingBoxes(coords))
						{
							if (negative)
							{
								if (entity.BoundingBox.Min.X - box.Max.X < 0)
									continue;
							}
							else
							{
								if (box.Min.X - entity.BoundingBox.Max.X < 0)
									continue;
							}
							
							if (testBox.Intersects(box))
							{
								if (climable && box.Max.Y > entity.BoundingBox.Min.Y && entity.KnownPosition.OnGround)
								{
									climable = CanClimb(entity.Velocity, entity.BoundingBox, box);
								}
								
								if (negative)
								{
									if ((collisionExtent == null || collisionExtent.Value < box.Max.X))
									{
										collisionExtent = box.Max.X;
										collisionPoint = coords;
										blockBox = box;
									}
								}
								else
								{
									if ((collisionExtent == null || collisionExtent.Value > box.Min.X))
									{
										collisionExtent = box.Min.X;
										collisionPoint = coords;
										blockBox = box;
									}
								}
							}
						}
					}
				}
			}

			if (collisionExtent != null) // Collision detected, adjust accordingly
			{
				var    extent = collisionExtent.Value;

				if (climable && entity.KnownPosition.OnGround)
				{
					var yDifference = blockBox.Max.Y - entity.BoundingBox.Min.Y;

					if (yDifference > 0f)
					{
						entity.Velocity = new Vector3(entity.Velocity.X, MathF.Sqrt(2f * (float) (entity.Gravity) * (yDifference)), entity.Velocity.Z);

						return false;
					}
				}
				
				if (entity.KnownPosition.OnGround && MathF.Abs(blockBox.Max.Y - testBox.Min.Y) < 0.005f)
				{
					return false;
				}

				
				float diff;
				
				if (negative)
					diff = -(entity.BoundingBox.Min.X - extent);
				else
					diff = (extent - entity.BoundingBox.Max.X);
				
				//Log.Warn($"Collision! Extent={extent} MinX={ entity.BoundingBox.Min.X} MaxX={ entity.BoundingBox.Max.X} Negative={negative} Diff={diff}");
				
				entity.Velocity = new Vector3(diff, entity.Velocity.Y, entity.Velocity.Z);
				return true;
			}
			
			return false;
		}

		private bool TestTerrainCollisionZ(Entity entity, out Vector3 collisionPoint, out BoundingBox blockBox)
		{
			collisionPoint = Vector3.Zero;
			blockBox = new BoundingBox();

			if (entity.Velocity.Z == 0)
				return false;

			bool negative;

			BoundingBox testBox;
		
			if (entity.Velocity.Z < 0)
			{
				testBox = new BoundingBox(
					new Vector3(
						entity.BoundingBox.Min.X, 
						entity.BoundingBox.Min.Y,
						entity.BoundingBox.Min.Z + entity.Velocity.Z),  
					entity.BoundingBox.Max);

				negative = true;
			}
			else
			{
				testBox = new BoundingBox(
					entity.BoundingBox.Min,
					new Vector3(
						entity.BoundingBox.Max.X,  
						entity.BoundingBox.Max.Y,
						entity.BoundingBox.Max.Z + entity.Velocity.Z)
					);

				negative = false;
			}

			float? collisionExtent = null;
			bool   climable        = true;

			for (int x = (int) (Math.Floor(testBox.Min.X)); x <= (int) (Math.Ceiling(testBox.Max.X)); x++)
			{
				for (int z = (int) (Math.Floor(testBox.Min.Z)); z <= (int) (Math.Ceiling(testBox.Max.Z)); z++)
				{
					for (int y = (int) (Math.Floor(testBox.Min.Y)); y <= (int) (Math.Ceiling(testBox.Max.Y)); y++)
					{
						var blockState = entity.Level.GetBlockState(x, y, z);
						if (blockState?.Model == null || !blockState.Block.Solid)
							continue;

						var coords = new Vector3(x, y, z);
						
						foreach (var box in blockState.Model.GetBoundingBoxes(coords))
						{
							if (negative)
							{
								if (entity.BoundingBox.Min.Z - box.Max.Z < 0)
									continue;
							}
							else
							{
								if (box.Min.Z - entity.BoundingBox.Max.Z < 0)
									continue;
							}
							
							if (testBox.Intersects(box))
							{
								if (climable && box.Max.Y > entity.BoundingBox.Min.Y && entity.KnownPosition.OnGround)
								{
									climable = CanClimb(entity.Velocity, entity.BoundingBox, box);
								}
								
								if (negative)
								{
									if ((collisionExtent == null || collisionExtent.Value < box.Max.Z))
									{
										collisionExtent = box.Max.Z;
										collisionPoint = coords;
										blockBox = box;
									}
								}
								else
								{
									if ((collisionExtent == null || collisionExtent.Value > box.Min.Z))
									{
										collisionExtent = box.Min.Z;
										collisionPoint = coords;
										blockBox = box;
									}
								}
							}
						}
					}
				}
			}

			if (collisionExtent != null) // Collision detected, adjust accordingly
			{
				
				var extent      = collisionExtent.Value;
				
				var yDifference = blockBox.Max.Y - entity.BoundingBox.Min.Y;
				if (climable && entity.KnownPosition.OnGround)
				{
					if (yDifference > 0f)
					{
						entity.Velocity = new Vector3(entity.Velocity.X, MathF.Sqrt(2f * (float) (entity.Gravity) * (yDifference )), entity.Velocity.Z);
						
						return false;
					}
				}

				if (entity.KnownPosition.OnGround && MathF.Abs(blockBox.Max.Y - testBox.Min.Y) < 0.005f)
				{
					return false;
				}
				
				float diff;
				
				if (negative)
					diff = -(entity.BoundingBox.Min.Z - extent);
				else
					diff = (extent - entity.BoundingBox.Max.Z);
				
		//		Log.Warn($"Collision! Extent={extent} MinZ={entity.BoundingBox.Min.Z} MaxZ={entity.BoundingBox.Max.Z} Negative={negative} Diff={diff}");
				
				entity.Velocity = new Vector3(entity.Velocity.X, entity.Velocity.Y, diff);
				return true;
			}
			
			return false;
		}
		
		private bool CanClimb(Vector3 velocity, BoundingBox entityBox, BoundingBox blockBox)
		{
			if (velocity.Y < 0f)
				return false;
			
			var yDifference = blockBox.Max.Y - entityBox.Min.Y;

			if (!(blockBox.Max.Y > entityBox.Min.Y)) 
				return false;

			if (yDifference > 0.55f)
				return false;

			if (GetIntersecting(World, entityBox).Any(bb => bb.Min.Y >= entityBox.Min.Y && bb.Min.Y <= entityBox.Max.Y))
				return false;

			return true;
		}

		private bool DetectOnGround(Entity e)
		{
			var entityBoundingBox =
				e.BoundingBox;
			
			var offset = 0f;

			//if (Math.Round(entityBoundingBox.Min.Y) <= (int) entityBoundingBox.Min.Y)
			if (entityBoundingBox.Min.Y % 1 < 0.05f)
			{
				offset = -1f;
			}

			bool foundGround = false;
			foreach (var corner in entityBoundingBox.GetCorners()
			   .Where(x => Math.Abs(x.Y - entityBoundingBox.Min.Y) < 0.001f))
			{

				var blockcoords = new BlockCoordinates(
					new PlayerLocation(corner.X, Math.Floor(corner.Y + offset), corner.Z));

				var block = World.GetBlockState(blockcoords.X, blockcoords.Y, blockcoords.Z);

				if (block?.Model == null || !block.Block.Solid)
					continue;

				foreach (var box in block.Model.GetBoundingBoxes(blockcoords).OrderBy(x => x.Max.Y))
				{
					var yDifference = MathF.Abs(entityBoundingBox.Min.Y - box.Max.Y); // <= 0.01f

					if (yDifference > 0.015f)
						continue;

					if (box.Intersects(entityBoundingBox))
						foundGround = true;
					//return true;
				}
			}

			return foundGround;
		}

		public BoundingBox[] LastKnownHit { get; set; } = null;

		public bool AddTickable(Entity entity)
	    {
		    return PhysicsEntities.TryAdd(entity);
	    }

	    public bool Remove(Entity entity)
	    {
		    return PhysicsEntities.Remove(entity);
	    }


	    private IEnumerable<BoundingBox> GetIntersecting(World world, BoundingBox box)
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

			    if (block == null)
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
