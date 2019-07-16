using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Alex.API.Entities;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.Minecraft;
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

	    public PhysicsManager(Alex alex, IWorld world)
	    {
		    Alex = alex;
		    World = world;
	    }

		private ThreadSafeList<IPhysicsEntity> PhysicsEntities { get; } = new ThreadSafeList<IPhysicsEntity>();

		private void TruncateVelocity(IPhysicsEntity entity, float dt)
		{
			if (Math.Abs(entity.Velocity.X) < 0.1 * dt)
				entity.Velocity = new Vector3(0, entity.Velocity.Y, entity.Velocity.Z);
			
			if (Math.Abs(entity.Velocity.Y) < 0.1 * dt)
				entity.Velocity = new Vector3(entity.Velocity.X, 0, entity.Velocity.Z);
			
			if (Math.Abs(entity.Velocity.Z) < 0.1 * dt)
				entity.Velocity = new Vector3(entity.Velocity.X, entity.Velocity.Y, 0);
			
			//entity.Velocity.Clamp();
		}

		Stopwatch sw = new Stopwatch();
		public void Update(GameTime elapsed)
		{
			float dt = (float) elapsed.ElapsedGameTime.TotalSeconds;
			//if (sw.ElapsedMilliseconds)
			//	dt = (float) sw.ElapsedMilliseconds / 1000f;

			Hit.Clear();
			foreach (var entity in PhysicsEntities.ToArray())
			{
				try
				{
					if (entity is Entity e)
					{
						if (e.NoAi) continue;
						//TruncateVelocity(e, dt);
						
						var velocity = e.Velocity;
						
						if (!e.IsFlying && !e.KnownPosition.OnGround)
						{
							velocity -= new Vector3(0, (float) (e.Gravity * dt), 0);
						}
						velocity *= (float)(1f - e.Drag * dt);
						
						var position = e.KnownPosition;

						var preview = position.PreviewMove(velocity * dt);

						var boundingBox = e.GetBoundingBox(preview);

						Bound bound = new Bound(World, boundingBox);
						
						velocity = AdjustForY(e.GetBoundingBox(new Vector3(position.X, preview.Y, position.Z)), bound,
							velocity, position);
						
						if (bound.GetIntersecting(boundingBox, out var blocks))
						{
							var solid = blocks.Where(b => b.block.Solid && b.box.Max.Y > position.Y).ToArray();
							Hit.AddRange(solid.Select(x => x.box));

							if (solid.Length > 0)
							{
								for (float x = 1f; x > 0f; x -= 0.1f)
								{
									Vector3 c = (position - preview) * x + position;
									if (solid.All(s => s.box.Contains(c) != ContainmentType.Contains))
									{
										velocity = new Vector3(c.X - position.X, velocity.Y, c.Z - position.Z);
										break;
									}
								}
							}
						}
						
						e.Velocity = velocity;

						e.KnownPosition.Move(e.Velocity * dt);
						
						TruncateVelocity(e, dt);

						if (velocity.Y == 0)
						{
							e.KnownPosition.OnGround = true;
						}
					}
				}
				catch (Exception ex)
				{
					Log.Warn(ex, $"Entity tick threw exception: {ex.ToString()}");
				}
			}

			if (Hit.Count > 0)
			{
				LastKnownHit = Hit.ToArray();
			}
			
			sw.Restart();
		}

		private Vector3 AdjustForY(BoundingBox box, Bound bound, Vector3 velocity, PlayerLocation position)
		{
			if (velocity.Y == 0f)
				return velocity;
			
			float? collisionPoint = null;
			bool negative = velocity.Y < 0f;
			foreach (var corner in box.GetCorners())
			{
				foreach (var block in bound.GetPoints())
				{
					if (block.block.Solid && block.box.Contains(corner) == ContainmentType.Contains)
					{
						var heading = corner - position;
						var distance = heading.LengthSquared();
						var direction = heading / distance;

						if (negative)
						{
							if (collisionPoint == null || block.box.Max.Y > collisionPoint.Value)
							{
								collisionPoint = block.box.Max.Y;
							}
						}
						else
						{
							if (collisionPoint == null || block.box.Min.Y < collisionPoint.Value)
							{
								collisionPoint = block.box.Min.Y;
							}
						}
					}
				}
			}

			if (collisionPoint.HasValue)
			{
				float distance = 0f;
				/*if (negative)
				{
					distance = -(box.Min.Y - collisionPoint.Value);
				}
				else
				{
					distance = collisionPoint.Value - box.Max.Y;
				}*/
				
				velocity = new Vector3(velocity.X, distance, velocity.Z);
			}

			return velocity;
		}
		
		public List<BoundingBox> Hit { get; set; } = new List<BoundingBox>();
		public BoundingBox[] LastKnownHit { get; set; } = null;
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

	    private class Bound
	    {
		    private Dictionary<BlockCoordinates, (Block block, BoundingBox box)> Blocks = new Dictionary<BlockCoordinates, (Block block, BoundingBox box)>();
		    
		    public Bound(IWorld world, BoundingBox box)
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
				    var coords = new BlockCoordinates(new Vector3(x,y,z));
				    if (!world.HasBlock(coords.X, coords.Y, coords.Z))
					    continue;
					    
				    if (!Blocks.TryGetValue(coords, out _))
				    {
					    var block = GetBlock(world, coords);
					    if (block != default)
					    Blocks.TryAdd(coords, block);
				    }
			    }
		    }

		    private (Block block, BoundingBox box) GetBlock(IWorld world, BlockCoordinates coordinates)
		    {
			    var block = world.GetBlock(coordinates) as Block;
			    if (block == null) return default;
			    
			    var box = block.GetBoundingBox(coordinates);

			    return (block, box);
		    }

		    public IEnumerable<(Block block, BoundingBox box)> GetPoints()
		    {
			    foreach (var b in Blocks)
			    {
				    yield return b.Value;
			    }
		    }

		    public bool GetIntersecting(BoundingBox box, out (Block block, BoundingBox box)[] blocks)
		    {
			    List<(Block block, BoundingBox box)> b = new List<(Block block, BoundingBox box)>();
			    foreach (var block in GetPoints())
			    {
				    if (block.box.Contains(box) == ContainmentType.Intersects)
				    {
					    b.Add(block);
				    }
			    }
			    
			    blocks = b.ToArray();
			    return (b.Count > 0);
		    }
		    
		    public bool Intersects(BoundingBox box, out Vector3 collisionPoint, out (Block block, BoundingBox box) block)
		    {
			    foreach (var point in GetPoints())
			    {
				    foreach (var corner in box.GetCorners())
				    {
					    if (point.box.Contains(corner) == ContainmentType.Contains)
					    {
						    collisionPoint = corner;
						    block = point;
						    return true;
					    }
				    }
			    }
			    
			    collisionPoint = default;
			    block = default;
			    return false;
		    }
	    }
    }
}
