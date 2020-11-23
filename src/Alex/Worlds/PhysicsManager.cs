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
    public class PhysicsManager : ITicked
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(PhysicsManager));
	    private World World { get; }

	    public PhysicsManager(World world)
	    {
		    World = world;
	    }

		private ThreadSafeList<IPhysicsEntity> PhysicsEntities { get; } = new ThreadSafeList<IPhysicsEntity>();
		
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
						
						var original = e.KnownPosition.ToVector3();
						
						var velocity = e.Velocity;
						velocity = UpdateEntity(e, velocity);
						
						e.KnownPosition.Move(velocity * dt);

						UpdateOnGround(e);

						e.DistanceMoved += MathF.Abs(Vector3.Distance(original, e.KnownPosition.ToVector3()));
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
			var entityBoundingBox = e.GetBoundingBox(new Vector3(e.KnownPosition.X, MathF.Round(e.KnownPosition.Y, 2), e.KnownPosition.Z));

			bool anySolid = false;

			var offset = 0f;

			if (Math.Round(entityBoundingBox.Min.Y, 2) % 1 == 0)
			{
				offset = 1f;
			}
						
			foreach (var corner in entityBoundingBox.GetCorners().Where(x => Math.Abs(x.Y - entityBoundingBox.Min.Y) < 0.001f))
			{
				var blockcoords = new BlockCoordinates(
					new PlayerLocation(
						corner.X, Math.Floor(corner.Y - offset), corner.Z));

				var block = World.GetBlockState(blockcoords.X, blockcoords.Y, blockcoords.Z);
				if (block?.Model == null || !block.Block.Solid)
					continue;

				foreach (var box in block.Model.GetBoundingBoxes(blockcoords))
				{
					//if (box.Max.Y > corner.Y)
					//	continue;
						
					var yDifference = MathF.Abs(entityBoundingBox.Min.Y - box.Max.Y);// <= 0.01f

					if (yDifference > 0.01f || box.Contains(new Vector3(corner.X, corner.Y - 0.015f, corner.Z)) == ContainmentType.Disjoint) 
						continue;
					
					anySolid = true;
					break;
				}
			}

			//e.KnownPosition.Y = yPos;
			e.KnownPosition.OnGround = anySolid;
		}

		private Stopwatch _timeSinceTick { get; set; } = new Stopwatch();
		public void OnTick()
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
					
					velocity = UpdateEntity(e, velocity);
						
					e.LastTickTime = DateTime.UtcNow;

					e.Velocity = velocity;
				}
			}
			
			_timeSinceTick.Restart();
		}

		internal Vector3 UpdateEntity(Entity e, Vector3 velocity)
		{
			List<BoundingBox> hit = new List<BoundingBox>();
		
			var original = e.KnownPosition.ToVector3();
			var originalPosition = e.KnownPosition;
			var isPlayer = e is Player;
			
			if (isPlayer)
				hit.Clear();
			
			var position = e.KnownPosition;

			var originalEntityBoundingBox = e.GetBoundingBox(position);
			
			var before = velocity;

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

				Bound bound = new Bound(World, bounding);

				var velocityBeforeAdjustment = new Vector3(velocity.X, velocity.Y, velocity.Z);

				if (bound.Boxes.Count > 0)
				{
					var   boxes = bound.Boxes.ToArray();
				//	Block collisionBlock;

					if (AdjustForY(
						e, e.GetBoundingBox(new Vector3(position.X, preview.Y, position.Z)), boxes, ref velocity,
						out var yCollisionPoint, ref position))
					{
						e.CollidedWithWorld(
							before.Y < 0 ? Vector3.Down : Vector3.Up, yCollisionPoint, velocityBeforeAdjustment.Y);
					}

					if (AdjustForX(
						e, e.GetBoundingBox(new Vector3(preview.X, position.Y, position.Z)), boxes, ref velocity,
						out var xCollisionPoint, ref position))
					{
						e.CollidedWithWorld(
							before.X < 0 ? Vector3.Left : Vector3.Right, xCollisionPoint, velocityBeforeAdjustment.X);
					}

					if (AdjustForZ(
						e, e.GetBoundingBox(new Vector3(position.X, position.Y, preview.Z)), boxes, ref velocity,
						out var zCollisionPoint, ref position))
					{
						e.CollidedWithWorld(
							before.Z < 0 ? Vector3.Backward : Vector3.Forward, zCollisionPoint,
							velocityBeforeAdjustment.Z);
					}

					if (isPlayer)
					{
						hit.AddRange(boxes);
					}
				}
			}

			/*if (before.Y >= 0f && position.Y > originalPosition.Y)
			{
				playerY = position.Y;
			}
			else
			{
				playerY = before.Y;	
			}*/

			if (isPlayer && hit.Count > 0)
			{
				LastKnownHit = hit.ToArray();
			}
			
			return velocity;
		}

		private bool CanClimb(Entity entity, BoundingBox entityBox, BoundingBox blockBox)
		{
			//newYPosition = entityBox.Min.Y;

			if (entity.Velocity.Y < 0f)
				return false;

			if (!(blockBox.Max.Y > entity.KnownPosition.Y)) 
				return false;

			if ((blockBox.Max.Y - entity.KnownPosition.Y) > 0.55f)
				return false;
				
			var pos = new Vector3(
				(entityBox.Min.X + entityBox.Max.X) / 2f, blockBox.Max.Y, (entityBox.Min.Z + entityBox.Max.Z) / 2f);

			var newEntityBoundingBox = entity.GetBoundingBox(pos);

			Bound bound = new Bound(World, newEntityBoundingBox);

			if (bound.Boxes.Any(bb => bb.Min.Y >= newEntityBoundingBox.Min.Y && bb.Min.Y <= newEntityBoundingBox.Max.Y))
				return false;

			//newYPosition = pos.Y + 0.075f;

			return true;
		}

		private bool AdjustForZ(Entity entity, BoundingBox box,
			BoundingBox[] blocks,
			ref Vector3 velocity, out Vector3 collisionPoint, ref PlayerLocation position)
		{
			collisionPoint = Vector3.Zero;

			float? collision = null;
			bool negative = velocity.Z < 0f;
			int blockIndex = 0;

			for (var index = 0; index < blocks.Length; index++)
			{
				var block = blocks[index];
				var blockBox = block;
				
				if (blockBox.Contains(box) != ContainmentType.Disjoint)
				{
				//	collisionBlock = block.block.Block;
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
				var bbox = blocks[blockIndex];
				if (bbox.Max.Y > position.Y && CanClimb(entity, box, bbox))
				{
					position.Y = bbox.Max.Y + 0.01f;
				}
				else
				{
					var dir = negative ? Vector3.Forward : Vector3.Backward;
					var vectorDistance = GetDistance(entity, position, collisionPoint, dir, negative);
					vectorDistance = TruncateVelocity(vectorDistance);
					
					velocity = new Vector3(velocity.X, velocity.Y, vectorDistance.Z);
				}

				return true;
			}

			return false;
		}

		private bool AdjustForX(Entity entity, BoundingBox box,
			BoundingBox[] blocks,
			ref Vector3 velocity, out Vector3 collisionPoint, ref PlayerLocation position)
		{
			collisionPoint = Vector3.Zero;

			float? collision = null;
			bool negative = velocity.X < 0f;
			var blockIndex = 0;

			for (var index = 0; index < blocks.Length; index++)
			{
				var block = blocks[index];
				var blockBox = block;

				if (blockBox.Contains(box) != ContainmentType.Disjoint)
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
				var bbox = blocks[blockIndex];
				if (bbox.Max.Y > position.Y && CanClimb(entity, box, bbox))
				{
					position.Y = bbox.Max.Y + 0.01f;
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

		private bool AdjustForY(Entity entity, BoundingBox box,
			BoundingBox[] blocks,
			ref Vector3 velocity, out Vector3 collisionPoint, ref PlayerLocation position)
		{
			collisionPoint = Vector3.Zero;
			float? pointOfCollision = null;
			bool negative = velocity.Y < 0f;
			int blockIndex = 0;
			
			for (var index = 0; index < blocks.Length; index++)
			{
				var block = blocks[index];
				var blockBox = block;

				if (blockBox.Contains(box) != ContainmentType.Disjoint)
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
				var bbox = blocks[blockIndex];
				if (bbox.Max.Y > position.Y && CanClimb(entity, box, bbox))
				{
					position.Y = bbox.Max.Y + 0.01f;
				}
				else
				{
					var vectorDistance = GetDistance(entity, position, collisionPoint, negative ? Vector3.Down : Vector3.Up, negative);
					vectorDistance = TruncateVelocity(vectorDistance);
					velocity = new Vector3(velocity.X, vectorDistance.Y, velocity.Z);
				}
				
				return true;
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
		
		public BoundingBox[] LastKnownHit { get; set; } = null;

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
		 //   private Dictionary<BlockCoordinates, (BlockState block, BoundingBox box)> Blocks = new Dictionary<BlockCoordinates, (BlockState block, BoundingBox box)>();
		    public List<BoundingBox> Boxes = new List<BoundingBox>();
		    public Bound(World world, BoundingBox box)
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

				    var block = world.GetBlockState(coords);
				    if (block == null)
					    continue;
				    
				    if (!block.Block.Solid)
					    continue;

				    foreach (var blockBox in block.Model.GetBoundingBoxes(coords))
				    {
					    if (box.Intersects(blockBox))
					    {
						    Boxes.Add(blockBox);
					    }
				    }
			    }
		    }
	    }
    }
}
