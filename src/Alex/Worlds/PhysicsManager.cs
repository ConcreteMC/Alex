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
    public class PhysicsManager : IDisposable
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(PhysicsManager));
	    private World World { get; }

	    public PhysicsManager(World world)
	    {
		    World = world;
	    }

		private ThreadSafeList<IPhysicsEntity> PhysicsEntities { get; } = new ThreadSafeList<IPhysicsEntity>();

		private void TruncateVelocity(IPhysicsEntity entity, float dt)
		{
			if (Math.Abs(entity.Velocity.X) < 0.005f)
				entity.Velocity = new Vector3(0, entity.Velocity.Y, entity.Velocity.Z);
			
			if (Math.Abs(entity.Velocity.Y) < 0.005f)
				entity.Velocity = new Vector3(entity.Velocity.X, 0, entity.Velocity.Z);
			
			if (Math.Abs(entity.Velocity.Z) < 0.005f)
				entity.Velocity = new Vector3(entity.Velocity.X, entity.Velocity.Y, 0);
			
			//entity.Velocity.Clamp();
		}

		Stopwatch sw = new Stopwatch();
		private Stopwatch _sw = Stopwatch.StartNew();
		public void Update(GameTime elapsed)
		{
			float dt = (float) elapsed.ElapsedGameTime.TotalMilliseconds / 50f;

			foreach (var entity in PhysicsEntities.ToArray())
			{
				try
				{
					if (entity is Entity e)
					{
						if (e.NoAi) continue;
						
						UpdateEntity(e, dt);
						
						TruncateVelocity(e, dt);

						var boundingBox = e.GetBoundingBox(e.KnownPosition);

						bool anySolid = false;
						foreach (var corner in boundingBox.GetCorners().Where(x => x.Y == boundingBox.Min.Y))
						{
							var blockcoords = new BlockCoordinates(
								new PlayerLocation(
									corner.X, Math.Floor(corner.Y - 0.01f), corner.Z));

							var block            = World.GetBlock(blockcoords.X, blockcoords.Y, blockcoords.Z);
							var blockBoundingBox = block.GetBoundingBox(blockcoords);

							//..onGround = onGround || block.Solid;

							if (block.Solid && MathF.Abs(boundingBox.Min.Y - blockBoundingBox.Max.Y) <= 0.05f)
							{
								anySolid = true;
								break;
							}
						}

						e.KnownPosition.OnGround = anySolid;
					}
				}
				catch (Exception ex)
				{
					Log.Warn(ex, $"Entity tick threw exception: {ex.ToString()}");
				}
			}

			sw.Restart();
		}

		private Stopwatch _timeSinceTick { get; set; } = new Stopwatch();
		public void Tick()
		{
			foreach (var entity in PhysicsEntities.ToArray())
			{
				if (entity is Entity e)
				{
					if (e.NoAi)
						continue;
					
					var blockcoords = e.KnownPosition.GetCoordinates3D().BlockDown();

					var block = World.GetBlock(blockcoords.X, blockcoords.Y, blockcoords.Z);
					float drag = (float) (1f - e.Drag);
					
					if (e.KnownPosition.OnGround)
					{
						var slipperiness = (float)block.BlockMaterial.Slipperiness;
						slipperiness *= 0.91f;
						var multiplier = (float)(0.1 * (0.1627714 / (slipperiness * slipperiness * slipperiness)));
						
						entity.Velocity *= new Vector3(slipperiness, 1f, slipperiness);
					}
					else if (!e.IsFlying && !e.KnownPosition.OnGround && e.IsAffectedByGravity)
					{
						drag = 0.91f;
						entity.Velocity -= new Vector3(0f, (float) (e.Gravity), 0f);

						//var modifier = new Vector3(1f, (float) (1f - (e.Gravity * dt)), 1f);
						//velocity *= modifier;
					}

					entity.Velocity *= new Vector3(drag, 0.98f, drag);

					//TruncateVelocity(e, 0f);
					
					//CheckCollision(e);
				}
			}
			
			_timeSinceTick.Restart();
		}

		private void UpdateEntity(Entity e, float deltaTime)
		{
			var originalPosition = e.KnownPosition;
			
			Hit.Clear();
			
			var position = e.KnownPosition;

			var originalEntityBoundingBox = e.GetBoundingBox(position);
			
			var before = e.Velocity;
			var velocity = e.Velocity;
			var preview = position.PreviewMove(velocity * deltaTime);
			var boundingBox = e.GetBoundingBox(preview);
			
			Bound bound = new Bound(World, boundingBox, preview);

			if (bound.GetIntersecting(boundingBox, out var blocks))
			{
				if (AdjustForY(
					originalEntityBoundingBox, e.GetBoundingBox(new Vector3(position.X, preview.Y, position.Z)), blocks,
					ref velocity, out var yCollisionPoint))
				{
					e.CollidedWithWorld(before.Y < 0 ? Vector3.Down : Vector3.Up, yCollisionPoint);
					//onGround = e.KnownPosition.OnGround;
				}

				var solidBlocks = blocks.Where(x => x.block.Solid).ToArray();

				if (solidBlocks.Length > 0)
				{
					if (!e.IsFlying && velocity.Y >= 0f)
					{
						var matchingBlocks = solidBlocks
						   .Where(x => x.block.Solid && x.box.Max.Y > preview.Y && x.block.CanCollide()).ToArray();

						if (matchingBlocks.Length > 0)
						{
							var closest = matchingBlocks.Min(x => x.box.Max.Y);

							if (MathF.Abs(preview.Y - closest) <= 0.55f)
							{
								e.KnownPosition.Y = closest + 0.005f;
								position.Y = closest + 0.005f;
							}
						}
					}

					if (AdjustForX(
						originalEntityBoundingBox, e.GetBoundingBox(new Vector3(preview.X, position.Y, position.Z)),
						blocks, ref velocity, out var xCollisionPoint))
					{
						e.CollidedWithWorld(before.X < 0 ? Vector3.Left : Vector3.Right, xCollisionPoint);
					}

					if (AdjustForZ(
						originalEntityBoundingBox, e.GetBoundingBox(new Vector3(position.X, position.Y, preview.Z)),
						blocks, ref velocity, out var zCollisionPoint))
					{
						e.CollidedWithWorld(before.Z < 0 ? Vector3.Backward : Vector3.Forward, zCollisionPoint);
					}

					Hit.AddRange(blocks.Select(x => x.box));
				}
			}

			if (Hit.Count > 0)
			{
				LastKnownHit = Hit.ToArray();
			}

			e.Velocity = velocity;
			
			e.KnownPosition.Move(e.Velocity * deltaTime);
			//e.KnownPosition.OnGround = onGround;

			e.DistanceMoved += MathF.Abs(Vector3.Distance(originalPosition, e.KnownPosition.ToVector3()));
		}

		private bool AdjustForZ(BoundingBox originalEntityBoundingBox, BoundingBox box, (BlockCoordinates coordinates, Block block, BoundingBox box, bool isBlockPart)[] blocks, ref Vector3 velocity, out Vector3 collisionPoint)
		{
			collisionPoint = Vector3.Zero;
			
			float? collision = null;
			bool   negative  = velocity.Z < 0f;
			foreach (var corner in box.GetCorners().OrderBy(x => x.Y).Where(
				x =>
				{
					if (negative)
						return x.Z <= originalEntityBoundingBox.Min.Z;

					return x.Z >= originalEntityBoundingBox.Max.Z;
				}))
			{
				foreach (var block in blocks)
				{
					var blockBox = block.box;
					
					bool pass = block.block.Solid && blockBox.Contains(corner) != ContainmentType.Disjoint;

					if (pass)
					{
						if (negative)
						{
							if (collision == null || collision.Value < blockBox.Max.Z)
							{
								collision = blockBox.Max.Z;
								collisionPoint = corner;
							}
						}
						else
						{
							if (collision == null || collision.Value > blockBox.Min.Z)
							{
								collision = blockBox.Min.Z;
								collisionPoint = corner;
							}
						}
					}
				}
			}
		
			if (collision.HasValue)
			{
				float distance = 0f;
				if (negative)
				{
					distance = -(originalEntityBoundingBox.Min.Z - collision.Value);
				}
				else
				{
					distance = collision.Value - originalEntityBoundingBox.Max.Z;
				}
				
				velocity = new Vector3(velocity.X, velocity.Y, distance);

				return true;
			}

			return false;
		}
		
		private bool AdjustForX(BoundingBox originalEntityBoundingBox, BoundingBox box, (BlockCoordinates coordinates, Block block, BoundingBox box, bool isBlockPart)[] blocks, ref Vector3 velocity, out Vector3 collisionPoint)
		{
			collisionPoint = Vector3.Zero;
			
			float? collision = null;
			bool   negative       = velocity.X < 0f;
			foreach (var corner in box.GetCorners().OrderBy(x => x.Y).Where(
				x =>
				{
					if (negative)
						return x.X <= originalEntityBoundingBox.Min.X;

					return x.X >= originalEntityBoundingBox.Max.X;
				}))
			{
				foreach (var block in blocks)
				{
					var blockBox = block.box;
					
					bool pass = block.block.Solid && blockBox.Contains(corner) != ContainmentType.Disjoint;

					if (pass)
					{
						if (negative)
						{
							if (collision == null || collision.Value < blockBox.Max.X)
							{
								collision = blockBox.Max.X;
								collisionPoint = corner;
							}
						}
						else
						{
							if (collision == null || collision.Value > blockBox.Min.X)
							{
								collision = blockBox.Min.X;
								collisionPoint = corner;
							}
						}
					}
				}
			}
		
			if (collision.HasValue)
			{
				float distance = 0f;
				if (negative)
				{
					distance = -(originalEntityBoundingBox.Min.X - collision.Value);
				}
				else
				{
					distance = collision.Value - originalEntityBoundingBox.Max.X;
				}
				
				velocity = new Vector3(distance, velocity.Y, velocity.Z);

				return true;
			}

			return false;
		}
		
		private bool AdjustForY(BoundingBox originalEntityBoundingBox, BoundingBox box, (BlockCoordinates coordinates, Block block, BoundingBox box, bool isBlockPart)[] blocks, ref Vector3 velocity, out Vector3 collisionPoint)
		{
			collisionPoint = Vector3.Zero;
			float? pointOfCollision = null;
			bool negative = velocity.Y < 0f;

			foreach (var corner in box.GetCorners().OrderBy(x => x.Y).Where(
				x =>
				{
					if (negative)
						return x.Y <= originalEntityBoundingBox.Min.Y;

					return x.Y >= originalEntityBoundingBox.Max.Y;
				}))
			{
				foreach (var block in blocks)
				{
					var blockBox = block.box;
					
					bool pass = block.block.Solid && blockBox.Contains(corner) != ContainmentType.Disjoint;

					if (pass)
					{
						if (negative)
						{
							if (pointOfCollision == null || pointOfCollision.Value < blockBox.Max.Y)
							{
								pointOfCollision = blockBox.Max.Y;
								collisionPoint = corner;
							}
						}
						else
						{
							if (pointOfCollision == null || pointOfCollision.Value > blockBox.Min.Y)
							{
								pointOfCollision = blockBox.Min.Y;
								collisionPoint = corner;
							}
						}
					}
				}
			}
			
			if (pointOfCollision.HasValue)
			{
				float distance = 0f;
				if (negative)
				{
					distance = -(originalEntityBoundingBox.Min.Y - pointOfCollision.Value);
				}
				else
				{
					distance = pointOfCollision.Value - originalEntityBoundingBox.Max.Y;
				}
				
				velocity = new Vector3(velocity.X, distance, velocity.Z);

				return true;
				//entity.CollidedWithWorld(new Vector3(0f,distance, 0f));
			}

			return false;
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
		    
		    public Bound(World world, BoundingBox box, Vector3 entityPos)
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
					    var block = GetBlock(world, coords, entityPos);
					    if (block != default)
					    Blocks.TryAdd(coords, block);
				    }
			    }
		    }

		    private (Block block, BoundingBox box) GetBlock(World world, BlockCoordinates coordinates, Vector3 entityPos)
		    {
			    var block = world.GetBlock(coordinates) as Block;
			    if (block == null) return default;
			    
			    //var entityBlockPos = new BlockCoordinates(entityPos);

			    var box = block.GetBoundingBox(coordinates);

			    //var height = (float)block.GetHeight(entityPos - box.Min);
			    //box.Max = new Vector3(box.Max.X, box.Min.Y + height, box.Max.Z);
			    return (block, box);
		    }

		    public IEnumerable<(Block block, BoundingBox box)> GetPoints()
		    {
			    foreach (var b in Blocks)
			    {
				    yield return b.Value;
			    }
		    }

		    public bool GetIntersecting(BoundingBox box, out (BlockCoordinates coordinates, Block block, BoundingBox box, bool isBlockPart)[] blocks)
		    {
			    List<(BlockCoordinates coordinates,Block block, BoundingBox box, bool isBlockPart)> b = new List<(BlockCoordinates coordinates,Block block, BoundingBox box, bool isBlockPart)>();
			    foreach (var block in Blocks)
			    {
				    var vecPos = new Vector3(block.Key.X, block.Key.Y, block.Key.Z);

				    if (block.Value.box.Intersects(box))
				    {
					    /*foreach (var intersect in block.Value.block.BlockState.Model.GetIntersecting(block.Key, box).OrderBy(x => x.Max.Y))
					    {
						    b.Add((block.Value.block, intersect));
						    break;
					    }*/

					    bool added = false;
					    var bb = block.Value.block.GetPartBoundingBox(block.Key, box);
					    if (bb.HasValue)
					    {
						    added = true;
						    b.Add((block.Key, block.Value.block, bb.Value, true));
						     /* foreach (var point in box.GetCorners().OrderBy(x => x.Y))
						      {
							    //  var bb = block.Value.block.GetPartBoundingBox(block.Key, point);
							     // if (!bb.HasValue)
								  //    continue;
							      
							      var bc = bb.Value.Contains(point);
							      if (bc == ContainmentType.Contains)
							      {
								      added = true;
								      b.Add((block.Key, block.Value.block, bb.Value, true));
								      // break;
							      }
						      }*/
					    }

					    if (!added)
					    {
						    var containmentType = block.Value.box.Contains(box);
						    if (containmentType == ContainmentType.Contains || containmentType == ContainmentType.Intersects)
						    {
							    //b.Add((block.Key, block.Value.block, block.Value.box, false));
						    }
						    //   b.Add((block.Key, block.Value.block, block.Value.box));
					    }
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
