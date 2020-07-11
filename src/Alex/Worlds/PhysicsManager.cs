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
			entity.Velocity = TruncateVelocity(entity.Velocity);
			/*if (Math.Abs(entity.Velocity.X) < 0.005f)
				entity.Velocity = new Vector3(0, entity.Velocity.Y, entity.Velocity.Z);
			
			if (Math.Abs(entity.Velocity.Y) < 0.005f)
				entity.Velocity = new Vector3(entity.Velocity.X, 0, entity.Velocity.Z);
			
			if (Math.Abs(entity.Velocity.Z) < 0.005f)
				entity.Velocity = new Vector3(entity.Velocity.X, entity.Velocity.Y, 0);
			*/
			//entity.Velocity.Clamp();
		}
		
		private Vector3 TruncateVelocity(Vector3 velocity)
		{
			if (Math.Abs(velocity.X) < 0.005f)
				velocity = new Vector3(0, velocity.Y, velocity.Z);
			
			if (Math.Abs(velocity.Y) < 0.005f)
				velocity = new Vector3(velocity.X, 0, velocity.Z);
			
			if (Math.Abs(velocity.Z) < 0.005f)
				velocity = new Vector3(velocity.X, velocity.Y, 0);

			return velocity;
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
						
						//if (!e.AlwaysTick && !e.IsRendered) continue;

						var original = e.KnownPosition.ToVector3();
						
						var velocity = e.Velocity;
						velocity = UpdateEntity(e, velocity, out var playerY);
						
						e.KnownPosition.Move(velocity * dt);

						if (playerY > e.KnownPosition.Y)
						{
							e.KnownPosition.Y = playerY;
						}

						UpdateOnGround(e);
						//e.KnownPosition.OnGround = onGround;

						e.DistanceMoved += MathF.Abs(Vector3.Distance(original, e.KnownPosition.ToVector3()));
						
						//TruncateVelocity(e, dt);
					}
				}
				catch (Exception ex)
				{
					Log.Warn(ex, $"Entity tick threw exception: {ex.ToString()}");
				}
			}

			sw.Restart();
		}

		private void UpdateOnGround(Entity e)
		{
			var entityBoundingBox = e.GetBoundingBox(new Vector3(e.KnownPosition.X, MathF.Round(e.KnownPosition.Y, 2, MidpointRounding.ToZero), e.KnownPosition.Z));

			bool anySolid = false;
			var  yPos     = e.KnownPosition.Y;
						
			var offset = 0f;

			if (Math.Round(yPos, 2) % 1 == 0)
			{
				offset = 1f;
			}
						
			foreach (var corner in entityBoundingBox.GetCorners().Where(x => Math.Abs(x.Y - entityBoundingBox.Min.Y) < 0.001f))
			{
				var blockcoords = new BlockCoordinates(
					new PlayerLocation(
						corner.X, Math.Floor(corner.Y - offset), corner.Z));

				var block            = World.GetBlock(blockcoords.X, blockcoords.Y, blockcoords.Z);
				var blockBoundingBox = block.GetBoundingBox(blockcoords);
							
				//..onGround = onGround || block.Solid;

				if (block.Solid && blockBoundingBox.Contains(corner) != ContainmentType.Disjoint)
				{
					var partBoundingBox = block.GetPartBoundingBox(blockcoords, entityBoundingBox);
					if (partBoundingBox.HasValue)
					{
						var yDifference = MathF.Abs(entityBoundingBox.Min.Y - partBoundingBox.Value.Max.Y);// <= 0.01f
						if (yDifference <= 0.01f)
						{
							anySolid = true;
							break;
						}
					}
				}
			}

			e.KnownPosition.Y = yPos;
			e.KnownPosition.OnGround = anySolid;
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
					
					var blockcoords = e.KnownPosition.GetCoordinates3D();//.BlockDown();

					if (Math.Round(e.KnownPosition.Y, 2) % 1 == 0)
					{
						blockcoords = blockcoords.BlockDown();
						//offset = 1f;
					}
					
					var block = World.GetBlock(blockcoords.X, blockcoords.Y, blockcoords.Z);
					float drag = (float) (1f - e.Drag);

					var velocity = e.Velocity;
					if (e.KnownPosition.OnGround)
					{
						var slipperiness = (float)block.BlockMaterial.Slipperiness;
						slipperiness *= 0.91f;
						var multiplier = (float)(0.1 * (0.1627714 / (slipperiness * slipperiness * slipperiness)));
						
						velocity *= new Vector3(slipperiness, 1f, slipperiness);
					}
					else if (!e.IsFlying && !e.KnownPosition.OnGround && e.IsAffectedByGravity)
					{
						drag = 0.91f;
						velocity -= new Vector3(0f, (float) (e.Gravity), 0f);

						//var modifier = new Vector3(1f, (float) (1f - (e.Gravity * dt)), 1f);
						//velocity *= modifier;
					}

					velocity *= new Vector3(drag, 0.98f, drag);

				//	if (!e.RequiresRealTimeTick)
					{
						//var dt = (float) ((DateTime.UtcNow - e.LastTickTime).TotalMilliseconds / 50f);
						
						velocity = UpdateEntity(e, velocity, out _);
						
						e.LastTickTime = DateTime.UtcNow;
					}

					e.Velocity = velocity;
					
					//UpdateOnGround(e);
					//TruncateVelocity(e, 0f);

					//CheckCollision(e);
				}
			}
			
			_timeSinceTick.Restart();
		}

		internal Vector3 UpdateEntity(Entity e, Vector3 velocity, out float playerY)
		{
			var original = e.KnownPosition.ToVector3();
			var originalPosition = e.KnownPosition;
			var isPlayer = e is Player;
			
			if (isPlayer)
				Hit.Clear();
			
			var position = e.KnownPosition;
			//var originalPosition = position;
			
			var originalEntityBoundingBox = e.GetBoundingBox(position);
			
			var before = velocity;
		//	var velocity = e.Velocity;

			if (e.HasCollision)
			{
				var preview     = position.PreviewMove(velocity);
				var boundingBox = e.GetBoundingBox(preview);

				var bounding = new BoundingBox(
					new Vector3(
						MathF.Min(originalEntityBoundingBox.Min.X, boundingBox.Min.X),
						MathF.Min(originalEntityBoundingBox.Min.Y, boundingBox.Min.Y),
						MathF.Min(originalEntityBoundingBox.Min.Z, boundingBox.Min.Z)),
					new Vector3(
						MathF.Max(originalEntityBoundingBox.Max.X, boundingBox.Max.X),
						MathF.Max(originalEntityBoundingBox.Max.Y, boundingBox.Max.Y),
						MathF.Max(originalEntityBoundingBox.Max.Z, boundingBox.Max.Z)));

				var modifiedPreview = preview;

				Bound bound = new Bound(World, bounding, preview);

				if (bound.GetIntersecting(bounding, false, out var blocks))
				{
					var solidBlocks = blocks.Where(x => x.block.Solid).ToArray();

					if (solidBlocks.Length > 0)
					{
						if (AdjustForY(
							e, originalEntityBoundingBox,
							e.GetBoundingBox(new Vector3(position.X, preview.Y, position.Z)), solidBlocks, ref velocity,
							out var yCollisionPoint, ref position))
						{
							e.CollidedWithWorld(before.Y < 0 ? Vector3.Down : Vector3.Up, yCollisionPoint);
						}

						if (AdjustForX(
							e, originalEntityBoundingBox,
							e.GetBoundingBox(new Vector3(preview.X, position.Y, position.Z)), solidBlocks, ref velocity,
							out var xCollisionPoint, ref position))
						{
							e.CollidedWithWorld(before.X < 0 ? Vector3.Left : Vector3.Right, xCollisionPoint);
						}

						if (AdjustForZ(
							e, originalEntityBoundingBox,
							e.GetBoundingBox(new Vector3(position.X, position.Y, preview.Z)), solidBlocks, ref velocity,
							out var zCollisionPoint, ref position))
						{
							e.CollidedWithWorld(before.Z < 0 ? Vector3.Backward : Vector3.Forward, zCollisionPoint);
						}

						if (isPlayer)
						{
							Hit.AddRange(solidBlocks.Select(x => x.box));
						}
					}
				}

				if (isPlayer && Hit.Count > 0)
				{
					LastKnownHit = Hit.ToArray();
				}
			}
			
			if (before.Y >= 0f && position.Y > originalPosition.Y)
			{
				playerY = position.Y;
			}
			else
			{
				playerY = before.Y;	
			}

			//e.Velocity = velocity;
			
			//e.KnownPosition.Move(e.Velocity * deltaTime);
			//e.KnownPosition.OnGround = onGround;

			//e.DistanceMoved += MathF.Abs(Vector3.Distance(original, e.KnownPosition.ToVector3()));

			return velocity;
		}

		private bool CanClimb(Entity entity, BoundingBox entityBox,
			(BlockCoordinates coordinates, Block block, BoundingBox box, bool isBlockPart) collisionBlock, out float newYPosition)
		{
			newYPosition = entityBox.Min.Y;
			if (entity.Velocity.Y < 0f)
				return false;
			
			if (collisionBlock.box.Max.Y > entity.KnownPosition.Y)
			{
				var difference = collisionBlock.box.Max.Y - entity.KnownPosition.Y;
				if (difference > 0.55f)
					return false;

				var h = entityBox.Max.Y - entityBox.Min.Y;
				var pos = new Vector3((entityBox.Min.X + entityBox.Max.X) / 2f, collisionBlock.box.Max.Y,
					(entityBox.Min.Z + entityBox.Max.Z) / 2f);
				var newEntityBoundingBox =
					entity.GetBoundingBox(pos);
				
				Bound bound = new Bound(World, newEntityBoundingBox, pos);
				if (bound.GetIntersecting(newEntityBoundingBox, false, out var collisionBlocks))
				{
					var solidBlocks = collisionBlocks.Where(x => x.block.Solid).ToArray();
					if (solidBlocks.Any(x => x.box.Min.Y > pos.Y && x.box.Min.Y < newEntityBoundingBox.Max.Y))
					{
						return false;
					}
				}

				newYPosition = pos.Y + 0.075f;
				return true;
			}

			return false;
		}

		private bool AdjustForZ(Entity entity, BoundingBox originalEntityBoundingBox, BoundingBox box,
			(BlockCoordinates coordinates, Block block, BoundingBox box, bool isBlockPart)[] blocks,
			ref Vector3 velocity, out Vector3 collisionPoint, ref PlayerLocation position)
		{
			collisionPoint = Vector3.Zero;

			float? collision = null;
			bool negative = velocity.Z < 0f;
			int blockIndex = 0;

			for (var index = 0; index < blocks.Length; index++)
			{
				var block = blocks[index];
				var blockBox = block.box;

				bool pass = block.block.Solid && blockBox.Contains(box) != ContainmentType.Disjoint;

				if (pass)
				{
					if (negative)
					{
						if (collision == null || collision.Value < blockBox.Max.Z)
						{
							collision = blockBox.Max.Z;
							collisionPoint = blockBox.Max;
							blockIndex = index;
						}
					}
					else
					{
						if (collision == null || collision.Value > blockBox.Min.Z)
						{
							collision = blockBox.Min.Z;
							collisionPoint = blockBox.Min;
							blockIndex = index;
						}
					}
				}
			}
			//}

			if (collision.HasValue)
			{
				if (CanClimb(entity, box, blocks[blockIndex], out float newYPosition) && newYPosition > position.Y)
				{
					position.Y = newYPosition;
				}
				else
				{
					var dir = negative ? Vector3.Forward : Vector3.Backward;
					var vectorDistance = GetDistance(entity, position, collisionPoint, dir, negative);
					vectorDistance = TruncateVelocity(vectorDistance);
					
					velocity = new Vector3(velocity.X, velocity.Y, vectorDistance.Z);
				}

				//velocity = new Vector3(velocity.X, velocity.Y, vectorDistance.Z);

				return true;
			}

			return false;
		}

		private bool AdjustForX(Entity entity, BoundingBox originalEntityBoundingBox, BoundingBox box,
			(BlockCoordinates coordinates, Block block, BoundingBox box, bool isBlockPart)[] blocks,
			ref Vector3 velocity, out Vector3 collisionPoint, ref PlayerLocation position)
		{
			collisionPoint = Vector3.Zero;

			float? collision = null;
			bool negative = velocity.X < 0f;
			var blockIndex = 0;

			for (var index = 0; index < blocks.Length; index++)
			{
				var block = blocks[index];
				var blockBox = block.box;

				bool pass = block.block.Solid && blockBox.Contains(box) != ContainmentType.Disjoint;

				if (pass)
				{
					if (negative)
					{
						if (collision == null || collision.Value < blockBox.Max.X)
						{
							collision = blockBox.Max.X;
							collisionPoint = blockBox.Max;

							blockIndex = index;
						}
					}
					else
					{
						if (collision == null || collision.Value > blockBox.Min.X)
						{
							collision = blockBox.Min.X;
							collisionPoint = blockBox.Min;

							blockIndex = index;
						}
					}
				}
			}
			//}

			if (collision.HasValue)
			{
				if (CanClimb(entity, box, blocks[blockIndex], out float newYPosition)&& newYPosition > position.Y)
				{
					position.Y = newYPosition;
				}
				else
				{
					var dir = negative ? Vector3.Left : Vector3.Right;
					var vectorDistance = GetDistance(entity, position, collisionPoint, dir, negative);
					vectorDistance = TruncateVelocity(vectorDistance);
					velocity = new Vector3(vectorDistance.X, velocity.Y, velocity.Z);
				}

				return true;
			}

			return false;
		}

		private bool AdjustForY(Entity entity, BoundingBox originalEntityBoundingBox, BoundingBox box,
			(BlockCoordinates coordinates, Block block, BoundingBox box, bool isBlockPart)[] blocks,
			ref Vector3 velocity, out Vector3 collisionPoint, ref PlayerLocation position)
		{
			collisionPoint = Vector3.Zero;
			float? pointOfCollision = null;
			bool negative = velocity.Y < 0f;
			int blockIndex = 0;
			
			for (var index = 0; index < blocks.Length; index++)
			{
				var block = blocks[index];
				var blockBox = block.box;

				bool pass = block.block.Solid && blockBox.Contains(box) != ContainmentType.Disjoint;

				if (pass)
				{
					if (negative)
					{
						if (pointOfCollision == null || pointOfCollision.Value < blockBox.Max.Y)
						{
							pointOfCollision = blockBox.Max.Y;
							collisionPoint = blockBox.Max;
							blockIndex = index;
						}
					}
					else
					{
						if (pointOfCollision == null || pointOfCollision.Value > blockBox.Min.Y)
						{
							pointOfCollision = blockBox.Min.Y;
							collisionPoint = blockBox.Min;
							blockIndex = index;
						}
					}
				}
			}
			//}

			if (pointOfCollision.HasValue)
			{
				if (CanClimb(entity, box, blocks[blockIndex], out float newYPosition)&& newYPosition > position.Y)
				{
					position.Y = newYPosition;
					//entity.KnownPosition.Y = newYPosition;
				}
				else
				{
					var vectorDistance = GetDistance(entity, position, collisionPoint, negative ? Vector3.Down : Vector3.Up, negative);
					vectorDistance = TruncateVelocity(vectorDistance);
					velocity = new Vector3(velocity.X, vectorDistance.Y, velocity.Z);
				}

				//velocity = new Vector3(velocity.X, vectorDistance.Y, velocity.Z);

				return true;
				//entity.CollidedWithWorld(new Vector3(0f,distance, 0f));
			}

			return false;
		}

		private Vector3 GetDistance(Entity entity, Vector3 entityPosition, Vector3 pointOfCollision, Vector3 direction, bool negative)
		{
			var halfWidth = ((float) entity.Width / 2f) * entity.Scale;
			var offset = new Vector3(halfWidth, 0f, halfWidth) * direction;

			if (direction == Vector3.Up)
			{
				offset = new Vector3(offset.X, (float) entity.Height * entity.Scale, offset.Z);
			}

			if (negative)
				return ((entityPosition + offset) - pointOfCollision) * direction;
			else
				return (pointOfCollision - (entityPosition + offset)) * direction;
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

		    public bool GetIntersecting(BoundingBox box, bool includeFullBlocks, out (BlockCoordinates coordinates, Block block, BoundingBox box, bool isBlockPart)[] blocks)
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
					    }

					    if (!added && includeFullBlocks)
					    {
						    /*var containmentType = block.Value.box.Contains(box);
						    if (containmentType == ContainmentType.Contains || containmentType == ContainmentType.Intersects)
						    {
							    //b.Add((block.Key, block.Value.block, block.Value.box, false));
						    }
						    //   b.Add((block.Key, block.Value.block, block.Value.box));*/
						    
							 b.Add((block.Key, block.Value.block, block.Value.box, false));
					    }
				    }
			    }
			    
			    blocks = b.ToArray();
			    return (b.Count > 0);
		    }
	    }
    }
}
