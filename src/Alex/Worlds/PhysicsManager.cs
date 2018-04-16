using System;
using System.Linq;
using System.Threading;
using Alex.API;
using Alex.API.Entities;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Entities;
using Alex.Utils;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Worlds
{
    public class PhysicsManager : IDisposable
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(PhysicsManager));

	    private Alex Alex { get; }
		private IWorld World { get; }

		private System.Threading.Timer Timer = null;// = new System.Threading.Timer(GameTick, null, 50, 50);
		private object _timerLock = new object();
	    public PhysicsManager(Alex alex, IWorld world)
	    {
		    Alex = alex;
		    World = world;
	    }

	    public void Start()
	    {
		    if (Timer == null)
		    {
			//    Timer = new System.Threading.Timer(GameTick, null, 50, 50);
		    }
		    else
		    {
			//    Timer.Change(50, 50);
		    }
	    }

		private ThreadSafeList<IPhysicsEntity> PhysicsEntities { get; } = new ThreadSafeList<IPhysicsEntity>();
 	    private long SkippedTicks = 0;
	    private void GameTick(object state)
	    {
		    if (!Monitor.TryEnter(_timerLock))
		    {
			    SkippedTicks++;
				Log.Warn($"Skipped {SkippedTicks} ticks, something is taking to long!");
				return;
		    }

		    SkippedTicks = 0;

			try
		    {
			    foreach (var tickable in PhysicsEntities.ToArray())
			    {
				    try
				    {
					   tickable.OnTick();
					}
				    catch (Exception ex)
				    {
						Log.Warn(ex, $"Entity tick threw exception: {ex.ToString()}");
				    }
			    }
		    }
		    finally
		    {
				Monitor.Exit(_timerLock);
		    }
	    }

	    private void TruncateVelocity(Entity entity, float multiplier)
	    {
		    if (Math.Abs(entity.Velocity.X) < 0.001 * multiplier)
			    entity.Velocity = new Vector3(0, entity.Velocity.Y, entity.Velocity.Z) ;

		    if (Math.Abs(entity.Velocity.Y) < 0.001 * multiplier)
				entity.Velocity = new Vector3(entity.Velocity.X, 0, entity.Velocity.Z);

		    if (Math.Abs(entity.Velocity.Z) < 0.001 * multiplier)
			    entity.Velocity = new Vector3(entity.Velocity.X, entity.Velocity.Y, 0);

		  /*  var groundSpeedSquared = entity.Velocity.X * entity.Velocity.X + entity.Velocity.Z * entity.Velocity.Z;

		    var maxSpeed = entity.IsFlagAllFlying ? (entity.IsSprinting ? 22f : 11f) : (entity.IsSprinting && !entity.IsSneaking ? 5.6f : (entity.IsSneaking ? 1.3 : 4.3f));
		    if (groundSpeedSquared > (maxSpeed))
		    {
			    var correctionScale = (float) Math.Sqrt(maxSpeed / groundSpeedSquared);
			    entity.Velocity *= new Vector3(correctionScale, 1f, correctionScale);
		    }*/

		   // entity.Velocity = Vector3.Clamp(entity.Velocity, entity.Velocity, new Vector3(entity.TerminalVelocity));// velocity;
		    entity.Velocity = Vector3.Clamp(entity.Velocity, -new Vector3(entity.TerminalVelocity), new Vector3(entity.TerminalVelocity));
	    }

		public void Update(GameTime elapsed)
	    {
		    float dt = (float)elapsed.ElapsedGameTime.TotalSeconds;

		    try
		    {
			    foreach (var entity in PhysicsEntities.ToArray())
			    {
				    try
				    {
					    if (entity is Entity e)
					    {
							if (e.NoAi) continue;
						    
							entity.Velocity -= new Vector3(0, (float) (entity.Gravity), 0);
						    entity.Velocity *= (float)(1f - entity.Drag);

						    TruncateVelocity(e, dt);

						    Vector3 collision, before = entity.Velocity;

						  //  var velocityInput = entity.Velocity * multiplier;

							if (TestTerrainCollisionY(e, entity.Velocity * dt, out collision))
							    e.TerrainCollision(collision, before.Y < 0 ? Vector3.Down : Vector3.Up);

						    if (TestTerrainCollisionX(e, entity.Velocity * dt, out collision))
							    e.TerrainCollision(collision, before.X < 0 ? Vector3.Left : Vector3.Right);

						    if (TestTerrainCollisionZ(e, entity.Velocity * dt, out collision))
							    e.TerrainCollision(collision, before.Z < 0 ? Vector3.Backward : Vector3.Forward);

						    //if (TestTerrainCollisionCylinder(e, entity.Velocity * dt, out collision))
							  //  e.TerrainCollision(collision, before);

							e.KnownPosition.Move(entity.Velocity * dt);

							TruncateVelocity(e, dt);
					    }
				    }
				    catch (Exception ex)
				    {
					    Log.Warn(ex, $"Entity tick threw exception: {ex.ToString()}");
				    }
			    }
		    }
		    finally
		    {
			  //  Monitor.Exit(_timerLock);
		    }
		}

	    private BoundingBox GetAABBVelocityBox(BoundingBox bbox, Vector3 velocity)
	    {
		    var min = new Vector3(
			    Math.Min(bbox.Min.X, bbox.Min.X + velocity.X),
			    Math.Min(bbox.Min.Y, bbox.Min.Y + velocity.Y),
			    Math.Min(bbox.Min.Z, bbox.Min.Z + velocity.Z)
		    );
		    var max = new Vector3(
			    Math.Max(bbox.Max.X, bbox.Max.X + velocity.X),
			    Math.Max(bbox.Max.Y, bbox.Max.Y + velocity.Y),
			    Math.Max(bbox.Max.Z, bbox.Max.Z + velocity.Z)
		    );

		    return new BoundingBox(min, max);
	    }

		private Vector3 AdjustVelocityForCollision(Vector3 velocity, BoundingBox entityBoundingBox, BoundingBox problem)
		{
			var boundingBox = entityBoundingBox;

			if (velocity.X < 0)
				velocity.X = boundingBox.Min.X - problem.Max.X;
			if (velocity.X > 0)
				velocity.X = boundingBox.Max.X - problem.Min.X;

			if (velocity.Y < 0)
				velocity.Y = boundingBox.Min.Y - problem.Max.Y;
			if (velocity.Y > 0)
				velocity.Y = boundingBox.Max.Y - problem.Min.Y;

			if (velocity.Z < 0)
				velocity.Z = boundingBox.Min.Z - problem.Max.Z;
			if (velocity.Z > 0)
				velocity.Z = boundingBox.Max.Z - problem.Min.Z;

			return velocity;
		}

		public bool TestTerrainCollisionCylinder(Entity entity, Vector3 velocity, out Vector3 collisionPoint)
		{
			collisionPoint = Vector3.Zero;
			var testBox = GetAABBVelocityBox(entity.BoundingBox, velocity);
			var testCylinder = new BoundingCylinder(testBox.Min, testBox.Max, entity.Width);//;.(entity.GetBoundingBox().Max));

			bool collision = false;
			for (int x = (int)(Math.Floor(testBox.Min.X)); x <= (int)(Math.Ceiling(testBox.Max.X)); x++)
			{
				for (int z = (int)(Math.Floor(testBox.Min.Z)); z <= (int)(Math.Ceiling(testBox.Max.Z)); z++)
				{
					for (int y = (int)(Math.Floor(testBox.Min.Y)); y <= (int)(Math.Ceiling(testBox.Max.Y)); y++)
					{
						var coords = new BlockCoordinates(x, y, z);

						var state = World.GetBlockState(x, y, z);
						var _box = state?.Model?.GetBoundingBox(new Vector3(x, y, z), state.Block);

						//var _box = BlockPhysicsProvider.GetBoundingBox(World, coords);
						if (_box == null)
							continue;

						if (!state.Block.Solid) continue;

						var box = _box.Value;
						if (testCylinder.Intersects(box))
						{
							if (testBox.Intersects(box))
							{
								collision = true;
								velocity = AdjustVelocityForCollision(velocity, entity.BoundingBox, box);
								testBox = GetAABBVelocityBox(entity.BoundingBox, velocity);
								testCylinder = new BoundingCylinder(testBox.Min, testBox.Max, entity.Width);
								collisionPoint = coords;
							}
						}
					}
				}
			}
			return collision;
		}

		public bool TestTerrainCollisionY(Entity entity, Vector3 velocity, out Vector3 collisionPoint)
		{
			// Things we need to do:
			// 1 - expand bounding box to include the destination and everything within
			// 2 - collect all blocks within that area
			// 3 - test bounding boxes in direction of motion

			collisionPoint = Vector3.Zero;

			if (velocity.Y == 0)
				return false;

			bool negative;

			BoundingBox testBox;
			if (velocity.Y < 0)
			{
				testBox = new BoundingBox(
					new Vector3(entity.BoundingBox.Min.X,
						entity.BoundingBox.Min.Y + velocity.Y,
						entity.BoundingBox.Min.Z),
					entity.BoundingBox.Max);
				negative = true;
			}
			else
			{
				testBox = new BoundingBox(
					entity.BoundingBox.Min,
					new Vector3(entity.BoundingBox.Max.X,
						entity.BoundingBox.Max.Y + velocity.Y,
						entity.BoundingBox.Max.Z));
				negative = false;
			}

			double? collisionExtent = null;
			for (int x = (int)(Math.Floor(testBox.Min.X)); x <= (int)(Math.Ceiling(testBox.Max.X)); x++)
			{
				for (int z = (int)(Math.Floor(testBox.Min.Z)); z <= (int)(Math.Ceiling(testBox.Max.Z)); z++)
				{
					for (int y = (int)(Math.Floor(testBox.Min.Y)); y <= (int)(Math.Ceiling(testBox.Max.Y)); y++)
					{
						var coords = new BlockCoordinates(x, y, z);
						var state = World.GetBlockState(x, y, z);

						var _box = state?.Model?.GetBoundingBox(new Vector3(x, y, z), state.Block);

						//var _box = BlockPhysicsProvider.GetBoundingBox(World, coords);
						if (_box == null)
							continue;

						if (!state.Block.Solid) continue;

						var box = _box.Value;
						if (testBox.Intersects(box))
						{
							if (negative)
							{
								if (collisionExtent == null || collisionExtent.Value < box.Max.Y)
								{
									collisionExtent = box.Max.Y;
									collisionPoint = coords;
								}
							}
							else
							{
								if (collisionExtent == null || collisionExtent.Value > box.Min.Y)
								{
									collisionExtent = box.Min.Y;
									collisionPoint = coords;
								}
							}
						}
					}
				}
			}

			if (collisionExtent != null) // Collision detected, adjust accordingly
			{
				var extent = collisionExtent.Value;
				double diff;
				if (negative)
					diff = -(entity.BoundingBox.Min.Y - extent);
				else
					diff = extent - entity.BoundingBox.Max.Y;
				entity.Velocity = new Vector3(entity.Velocity.X, (float) diff, entity.Velocity.Z);
				return true;
			}
			return false;
		}

		public bool TestTerrainCollisionX(Entity entity, Vector3 velocity, out Vector3 collisionPoint)
		{
			// Things we need to do:
			// 1 - expand bounding box to include the destination and everything within
			// 2 - collect all blocks within that area
			// 3 - test bounding boxes in direction of motion

			collisionPoint = Vector3.Zero;

			if (velocity.X == 0)
				return false;

			bool negative;

			BoundingBox testBox;
			if (velocity.X < 0)
			{
				testBox = new BoundingBox(
					new Vector3(
						entity.BoundingBox.Min.X + velocity.X,
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
						entity.BoundingBox.Max.X + velocity.X,
						entity.BoundingBox.Max.Y,
						entity.BoundingBox.Max.Z));
				negative = false;
			}

			double? collisionExtent = null;
			for (int x = (int)(Math.Floor(testBox.Min.X)); x <= (int)(Math.Ceiling(testBox.Max.X)); x++)
			{
				for (int z = (int)(Math.Floor(testBox.Min.Z)); z <= (int)(Math.Ceiling(testBox.Max.Z)); z++)
				{
					for (int y = (int)(Math.Floor(testBox.Min.Y)); y <= (int)(Math.Ceiling(testBox.Max.Y)); y++)
					{
						var coords = new BlockCoordinates(x, y, z);
						//if (!World.IsValidPosition(coords))
						//	continue;

						var state = World.GetBlockState(x, y, z);

						var _box = state?.Model?.GetBoundingBox(new Vector3(x, y, z), state.Block);

						//var _box = BlockPhysicsProvider.GetBoundingBox(World, coords);
						if (_box == null)
							continue;

						if (!state.Block.Solid) continue;

						var box = _box.Value;
						if (testBox.Intersects(box))
						{
							if (negative)
							{
								if (collisionExtent == null || collisionExtent.Value < box.Max.X)
								{
									collisionExtent = box.Max.X;
									collisionPoint = coords;
								}
							}
							else
							{
								if (collisionExtent == null || collisionExtent.Value > box.Min.X)
								{
									collisionExtent = box.Min.X;
									collisionPoint = coords;
								}
							}
						}
					}
				}
			}

			if (collisionExtent != null) // Collision detected, adjust accordingly
			{
				var extent = collisionExtent.Value;
				double diff;
				if (negative)
					diff = -(entity.BoundingBox.Min.X - extent);
				else
					diff = extent - entity.BoundingBox.Max.X;
				entity.Velocity = new Vector3((float)diff, entity.Velocity.Y, entity.Velocity.Z);
				return true;
			}
			return false;
		}

		public bool TestTerrainCollisionZ(Entity entity, Vector3 velocity, out Vector3 collisionPoint)
		{
			// Things we need to do:
			// 1 - expand bounding box to include the destination and everything within
			// 2 - collect all blocks within that area
			// 3 - test bounding boxes in direction of motion

			collisionPoint = Vector3.Zero;

			if (velocity.Z == 0)
				return false;

			bool negative;

			BoundingBox testBox;
			if (velocity.Z < 0)
			{
				testBox = new BoundingBox(
					new Vector3(
						entity.BoundingBox.Min.X,
						entity.BoundingBox.Min.Y,
						entity.BoundingBox.Min.Z + velocity.Z),
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
						entity.BoundingBox.Max.Z + velocity.Z));
				negative = false;
			}

			double? collisionExtent = null;
			for (int x = (int)(Math.Floor(testBox.Min.X)); x <= (int)(Math.Ceiling(testBox.Max.X)); x++)
			{
				for (int z = (int)(Math.Floor(testBox.Min.Z)); z <= (int)(Math.Ceiling(testBox.Max.Z)); z++)
				{
					for (int y = (int)(Math.Floor(testBox.Min.Y)); y <= (int)(Math.Ceiling(testBox.Max.Y)); y++)
					{
						var coords = new BlockCoordinates(x, y, z);
						//if (!World.IsValidPosition(coords))
						//	continue;

						var state = World.GetBlockState(x, y, z);
						
						var _box = state?.Model?.GetBoundingBox(new Vector3(x, y, z), state.Block);

						//var _box = BlockPhysicsProvider.GetBoundingBox(World, coords);
						if (_box == null)
							continue;

						if (!state.Block.Solid) continue;

						var box = _box.Value;
						//var box = _box.Value.OffsetBy(coords);
						if (testBox.Intersects(box))
						{
							if (negative)
							{
								if (collisionExtent == null || collisionExtent.Value < box.Max.Z)
								{
									collisionExtent = box.Max.Z;
									collisionPoint = coords;
								}
							}
							else
							{
								if (collisionExtent == null || collisionExtent.Value > box.Min.Z)
								{
									collisionExtent = box.Min.Z;
									collisionPoint = coords;
								}
							}
						}
					}
				}
			}

			if (collisionExtent != null) // Collision detected, adjust accordingly
			{
				var extent = collisionExtent.Value;
				double diff;
				if (negative)
					diff = -(entity.BoundingBox.Min.Z - extent);
				else
					diff = extent - entity.BoundingBox.Max.Z;
				entity.Velocity = new Vector3(entity.Velocity.X, entity.Velocity.Y, (float)diff);
				return true;
			}
			return false;
		}

		public void Stop()
	    {
		  //  Timer.Change(Timeout.Infinite, Timeout.Infinite);
	    }

	    public void Dispose()
	    {
		   // Timer?.Dispose();
	    }

	    public bool AddTickable(IPhysicsEntity entity)
	    {
		    return PhysicsEntities.TryAdd(entity);
	    }

	    public bool Remove(IPhysicsEntity entity)
	    {
		    return PhysicsEntities.Remove(entity);
	    }
    }
}
