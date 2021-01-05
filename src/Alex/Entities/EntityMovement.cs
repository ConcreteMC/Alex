using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities
{
	/// <summary>
	/// 	TODO: Implement interpolated movement by seperating KnownPosition from RenderPosition
	/// </summary>
	public class EntityMovement : ITicked
	{
		public Entity  Entity  { get; }
		public Vector3 Heading { get; private set; }

		public bool InterpolatedMovement { get; set; } = true;
		public EntityMovement(Entity entity)
		{
			Entity = entity;
			Heading = Vector3.Zero;
		}

		private object _headingLock = new object();
		public void UpdateHeading(Vector3 heading)
		{
			lock (_headingLock)
			{
				Heading = Vector3.Transform(heading, Matrix.CreateRotationY(-MathHelper.ToRadians(Entity.KnownPosition.HeadYaw)));;
			}
		}
		
		public void MoveTo(PlayerLocation location, bool updateLook = true)
		{
			var distance = Microsoft.Xna.Framework.Vector3.Distance(
				Entity.KnownPosition.ToVector3() * new Vector3(1f, 0f, 1f), location.ToVector3() * new Vector3(1f, 0f, 1f));

			//var difference = Entity.KnownPosition.ToVector3() - location.ToVector3();
			//Move(difference);
			
			Entity.KnownPosition = location;

			//Entity.KnownPosition.X = location.X;
			//Entity.KnownPosition.Y = location.Y;
			//Entity.KnownPosition.Z = location.Z;
			//Entity.KnownPosition.OnGround = location.OnGround;

			if (updateLook)
			{
				//Entity.KnownPosition.Yaw = location.Yaw;
				//Entity.KnownPosition.HeadYaw = location.HeadYaw;
				//Entity.KnownPosition.Pitch = location.Pitch;
			}
			
			UpdateTarget();

			Entity.DistanceMoved += MathF.Abs(distance);
		}

		public Vector3 Move(Vector3 amount)
		{
			var   oldPosition = Entity.KnownPosition.ToVector3();
			float offset      = 0f;

			//TODO: Fix position offset

			if (Entity.HasCollision)
			{
				var beforeAdjustment = new Vector3(amount.X, amount.Y, amount.Z);

				List<BoundingBox> boxes = new List<BoundingBox>();

				if (TestTerrainCollisionY(ref amount, out var yCollisionPoint, out var yBox, boxes))
				{
					if (beforeAdjustment.Y <= 0f)
						Entity.KnownPosition.OnGround = true;

					Entity.Velocity *= new Vector3(1f, 0f, 1f);
					//Entity.Velocity += new Vector3(0f, direction.Y, 0f);

					Entity.CollidedWithWorld(
						beforeAdjustment.Y < 0 ? Vector3.Down : Vector3.Up, yCollisionPoint, beforeAdjustment.Y);
				}

				float collisionX = 0f;

				bool collideX = TestTerrainCollisionX(
					ref amount, out var xCollisionPoint, out var xBox, out collisionX, boxes);

				float collisionZ = 0f;

				bool collideZ = TestTerrainCollisionZ(
					ref amount, out var zCollisionPoint, out var zBox, out collisionZ, boxes);

				var canJump = false;

				if (Entity.KnownPosition.OnGround)
				{
					canJump = true;
					var adjusted     = Entity.GetBoundingBox(Entity.KnownPosition + amount);
					var intersecting = PhysicsManager.GetIntersecting(Entity.Level, adjusted);
					var targetY      = 0f;

					//if (!PhysicsManager.GetIntersecting(Entity.Level, adjusted).Any(bb => bb.Max.Y >= adjusted.Min.Y && bb.Min.Y <= adjusted.Max.Y))
					foreach (var box in intersecting)
					{
						var yDifference = box.Max.Y - Entity.BoundingBox.Min.Y;

						if (yDifference > MaxJumpHeight)
						{
							canJump = false;

							break;
						}

						if (yDifference > targetY)
							targetY = yDifference;
					}

					if (canJump && targetY > 0f)
					{
						var originalY = amount.Y;
						amount.Y = targetY;
						//var a = intersecting.
						adjusted     = Entity.GetBoundingBox(Entity.KnownPosition + amount);

						if (PhysicsManager.GetIntersecting(Entity.Level, adjusted).Any(
							bb => bb.Max.Y > adjusted.Min.Y && bb.Min.Y <= adjusted.Max.Y))
						{
							canJump = false;
							amount.Y = originalY;
						}
					}
					else
					{
						canJump = false;
					}
				}

				if (!canJump)
				{
					if (collideX)
					{
						//Entity.Velocity += new Vector3(direction.X, 0f, 0f);

						Entity.CollidedWithWorld(
							beforeAdjustment.X < 0 ? Vector3.Left : Vector3.Right, xCollisionPoint, beforeAdjustment.X);

						var dir = (xCollisionPoint - Entity.KnownPosition);
						dir.Normalize();
						var block = Entity.Level.GetBlockState(xCollisionPoint);

						if (block != null && block.Block.CanClimb(dir.GetBlockFace()))
						{
							//amount.Y += MathF.Abs(beforeAdjustment.X);
							amount.X = 0;
							amount.Y = Math.Max(amount.Y, beforeAdjustment.X * 0.3f);
							Entity.Velocity = new Vector3(0f, Entity.Velocity.Y, Entity.Velocity.Z);
						}
						else
						{
							amount.X = collisionX;

							Entity.Velocity = new Vector3(0f, Entity.Velocity.Y, Entity.Velocity.Z);
						}
					}

					if (collideZ)
					{
						amount.Z = collisionZ;
						Entity.Velocity = new Vector3(Entity.Velocity.X, Entity.Velocity.Y, 0f);
						//Entity.Velocity *= new Vector3(1f, 1f, 0f);
						//Entity.Velocity += new Vector3(0f, 0f, direction.Z);

						Entity.CollidedWithWorld(
							beforeAdjustment.Z < 0 ? Vector3.Backward : Vector3.Forward, zCollisionPoint,
							beforeAdjustment.Z);

						var dir = (zCollisionPoint - Entity.KnownPosition);
						dir.Normalize();

						var block = Entity.Level.GetBlockState(zCollisionPoint);

						if (block != null && block.Block.CanClimb(dir.GetBlockFace()))
						{
							amount.Z = 0;
							amount.Y = Math.Max(amount.Y, beforeAdjustment.Z * 0.3f);
							Entity.Velocity = new Vector3(Entity.Velocity.X, Entity.Velocity.Y, 0f);
						}
						else
						{
							amount.Z = collisionZ;

							Entity.Velocity = new Vector3(Entity.Velocity.X, Entity.Velocity.Y, 0f);
						}
					}
				}
				
				if (boxes.Count > 0)
				{
					LastCollision = boxes.ToArray();
				}
			}

			Entity.KnownPosition += amount;
			//Entity.KnownPosition.Y += (amount.Y - offset);
			//Entity.KnownPosition.Z += amount.Z;
			
			Entity.KnownPosition.OnGround = DetectOnGround();
			
			//Entity.Velocity = direction;
			
			UpdateTarget();
			
			Entity.DistanceMoved +=
				MathF.Abs(Microsoft.Xna.Framework.Vector3.Distance(oldPosition , Entity.KnownPosition.ToVector3()));

			return amount;
		}

		public BoundingBox[] LastCollision { get; private set; } = new BoundingBox[0];
		
		private PlayerLocation _from;
		private PlayerLocation _target;
		private void UpdateTarget()
		{
			var target = Entity.KnownPosition;

			if (!InterpolatedMovement)
			{
				Entity.RenderLocation = target;
				return;
			}
			
			var distance = Microsoft.Xna.Framework.Vector3.DistanceSquared(
				Entity.RenderLocation.ToVector3() * new Vector3(1f, 0f, 1f), target.ToVector3() * new Vector3(1f, 0f, 1f));

			if (distance >= 16f)
			{
				Entity.RenderLocation = target;
				_frameAccumulator = TargetTime;
			}
			else
			{
				_frameAccumulator = 0;
				_from = (PlayerLocation) Entity.RenderLocation.Clone();
				_target = (PlayerLocation) target.Clone();
			}
		}

		public void Push(Vector3 velocity)
		{
			Entity.Velocity += velocity;
		}

		public void Velocity(Vector3 velocity)
		{
			var oldLength = (Entity.Velocity).Length();
			if (oldLength < velocity.Length())
			{
				Entity.Velocity += new Vector3(velocity.X - Entity.Velocity.X, velocity.Y - Entity.Velocity.Y, velocity.Z - Entity.Velocity.Z);
			}
			else
			{
				Entity.Velocity = new Vector3(
					MathF.Abs(Entity.Velocity.X) < 0.0001f ? velocity.Y : Entity.Velocity.X,
					MathF.Abs(Entity.Velocity.Y) < 0.0001f ? velocity.Y : Entity.Velocity.Y,
					MathF.Abs(Entity.Velocity.Z) < 0.0001f ? velocity.Z : Entity.Velocity.Z);
			}
		}
		
		public void Teleport(PlayerLocation location)
		{
			Entity.KnownPosition = location;
			/*
			var oldPosition = entity.KnownPosition;
			entity.KnownPosition = position;
			//if (entity is PlayerMob p)
			{
				entity.DistanceMoved += MathF.Abs(Vector3.Distance(oldPosition, position));
			}*/
		}

		private       float _frameAccumulator = 0f;
		private const float TargetTime        = 1f / 20f;
		public void Update(GameTime gt)
		{
			var entity    = Entity;

			if ((_target == null || _from == null))
			{
				UpdateTarget();
				return;
			}

			if (_frameAccumulator >= TargetTime)
				return;

			var frameTime = (float) gt.ElapsedGameTime.TotalSeconds; // / 50;
			_frameAccumulator += frameTime;

			var alpha                 = (float) (_frameAccumulator / TargetTime);
			alpha = MathF.Min(1f, MathF.Max(alpha, 0f));
			
			var targetPosition        = _target;
			var previousStatePosition = _from;
			
			var previousYaw     = previousStatePosition.Yaw;
			var previousHeadYaw = previousStatePosition.HeadYaw;
			var previousPitch   = previousStatePosition.Pitch;
			
			var targetYaw             = targetPosition.Yaw;
			var targetHeadYaw         = targetPosition.HeadYaw;
			var targetPitch           = targetPosition.Pitch;

			//var pos = Vector3.Lerp(previousStatePosition.ToVector3(), position.ToVector3(), alpha);
			var pos = targetPosition.ToVector3() * alpha + previousStatePosition.ToVector3() * (1f - alpha);

			//var yaw = targetPosition.Yaw;
			//var yaw = MathHelper.Lerp(previousStatePosition.Yaw, targetPosition.Yaw, alpha);
			var yaw = targetYaw * alpha + previousYaw * (1f - alpha);

			//var headYaw = targetPosition.HeadYaw;
			//var headYaw = MathHelper.Lerp(previousStatePosition.HeadYaw, targetPosition.HeadYaw, alpha);
			//var headYawDifference = MathF.Abs(targetHeadYaw - previousHeadYaw);
			var headYaw          = targetHeadYaw * alpha + previousHeadYaw * (1f - alpha);

			//var pitch = targetPosition.Pitch;
			var pitch = targetPitch * alpha + previousPitch * (1f - alpha);
			//var pitch = MathHelper.Lerp(previousStatePosition.Pitch, targetPosition.Pitch, alpha);

			var renderLocation = entity.RenderLocation;
			
			renderLocation.X = pos.X;
			renderLocation.Y = pos.Y;
			renderLocation.Z = pos.Z;

			renderLocation.HeadYaw = headYaw;
			renderLocation.Yaw = yaw;
			renderLocation.Pitch = pitch;
			
			renderLocation.OnGround = targetPosition.OnGround;

			entity.RenderLocation = renderLocation;
		}

		/// <inheritdoc />
		public void OnTick()
		{
			//UpdateTarget();
		}
		
		private bool DetectOnGround()
		{
			var entityBoundingBox =
				Entity.BoundingBox;
			
			var offset = 0f;

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

				var block = Entity.Level.GetBlockState(blockcoords.X, blockcoords.Y, blockcoords.Z);

				if (block?.Model == null || !block.Block.Solid)
					continue;

				foreach (var box in block.Model.GetBoundingBoxes(blockcoords).OrderBy(x => x.Max.Y))
				{
					var yDifference = MathF.Abs(entityBoundingBox.Min.Y - box.Max.Y); // <= 0.01f

					if (yDifference > 0.015f)
						continue;

					if (box.Contains(corner) == ContainmentType.Contains)
						foundGround = true;
					//return true;
				}
			}

			return foundGround;
		}
		
		private bool TestTerrainCollisionY(ref Vector3 velocity, out Vector3 collisionPoint, out BoundingBox blockBox, List<BoundingBox> boxes)
		{
			collisionPoint = Vector3.Zero;
			blockBox = new BoundingBox();
			
			if (velocity.Y == 0)
				return false;

			bool negative;

			BoundingBox testBox;
			if (velocity.Y < 0)
			{
				testBox = new BoundingBox(
					new Vector3(
						Entity.BoundingBox.Min.X,  Entity.BoundingBox.Min.Y + velocity.Y,
						Entity.BoundingBox.Min.Z),  Entity.BoundingBox.Max);

				negative = true;
			}
			else
			{
				testBox = new BoundingBox(
					Entity.BoundingBox.Min,
					new Vector3(
						Entity.BoundingBox.Max.X,  Entity.BoundingBox.Max.Y + velocity.Y,
						Entity.BoundingBox.Max.Z));

				negative = false;
			}

			float? collisionExtent = null;

			for (int x = (int) (Math.Floor(testBox.Min.X)); x <= (int) (Math.Ceiling(testBox.Max.X)); x++)
			{
				for (int z = (int) (Math.Floor(testBox.Min.Z)); z <= (int) (Math.Ceiling(testBox.Max.Z)); z++)
				{
					for (int y = (int) (Math.Floor(testBox.Min.Y)); y <= (int) (Math.Ceiling(testBox.Max.Y)); y++)
					{
						var blockState = Entity.Level.GetBlockState(x, y, z);
						if (blockState?.Model == null || !blockState.Block.Solid)
							continue;

						var coords = new Vector3(x, y, z);
						
						foreach (var box in blockState.Model.GetBoundingBoxes(coords))
						{
							if (negative)
							{
								if (Entity.BoundingBox.Min.Y - box.Max.Y < 0)
									continue;
							}
							else
							{
								if (box.Min.Y - Entity.BoundingBox.Max.Y < 0)
									continue;
							}
							
							if (testBox.Intersects(box))
							{
								boxes.Add(box);
								
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

				float diff;
				if (negative)
					diff = -(Entity.BoundingBox.Min.Y - extent);
				else
					diff = extent - Entity.BoundingBox.Max.Y;

				velocity.Y = (float)diff;	
				
				return true;
			}
			
			return false;
		}

		private bool TestTerrainCollisionX(ref Vector3 velocity, out Vector3 collisionPoint, out BoundingBox blockBox, out float result, List<BoundingBox> boxes)
		{
			result = velocity.X;
			collisionPoint = Vector3.Zero;
			blockBox = new BoundingBox();

			if (velocity.X == 0)
				return false;
			
			bool negative;

			BoundingBox testBox;

			if (velocity.X < 0)
			{
				testBox = new BoundingBox(
					new Vector3(
						Entity.BoundingBox.Min.X + velocity.X, 
						Entity.BoundingBox.Min.Y,
						Entity.BoundingBox.Min.Z),
					Entity.BoundingBox.Max);

				negative = true;
			}
			else
			{
				testBox = new BoundingBox(
					Entity.BoundingBox.Min,
					new Vector3(
						Entity.BoundingBox.Max.X + velocity.X, 
						Entity.BoundingBox.Max.Y,
						Entity.BoundingBox.Max.Z));

				negative = false;
			}

			float?            collisionExtent = null;

			for (int x = (int) (Math.Floor(testBox.Min.X)); x <= (int) (Math.Ceiling(testBox.Max.X)); x++)
			{
				for (int z = (int) (Math.Floor(testBox.Min.Z)); z <= (int) (Math.Ceiling(testBox.Max.Z)); z++)
				{
					for (int y = (int) (Math.Floor(testBox.Min.Y)); y <= (int) (Math.Ceiling(testBox.Max.Y)); y++)
					{
						var blockState = Entity.Level.GetBlockState(x, y, z);
						if (blockState?.Model == null || !blockState.Block.Solid)
							continue;

						var coords = new Vector3(x, y, z);
						
						foreach (var box in blockState.Model.GetBoundingBoxes(coords))
						{
							if (box.Max.Y <= testBox.Min.Y) continue;
							
							if (negative)
							{
								if (box.Max.X <= testBox.Min.X)
									continue;
								
								if (Entity.BoundingBox.Min.X - box.Max.X < 0)
									continue;
							}
							else
							{
								if (box.Min.Z >= testBox.Max.Z)
									continue;
								
								if (box.Min.X - Entity.BoundingBox.Max.X < 0)
									continue;
							}
							
							if (testBox.Intersects(box))
							{
								boxes.Add(box);

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
				double diff;
				
				if (negative)
					diff = -(Entity.BoundingBox.Min.X - collisionExtent.Value);
				else
					diff = (collisionExtent.Value - Entity.BoundingBox.Max.X);

				result = (float) diff;

				return true;
			}

			return false;
		}

		private bool TestTerrainCollisionZ(ref Vector3 velocity, out Vector3 collisionPoint, out BoundingBox blockBox, out float result, List<BoundingBox> boxes)
		{
			result = velocity.Z;
			collisionPoint = Vector3.Zero;
			blockBox = new BoundingBox();

			if (velocity.Z == 0)
				return false;

			bool negative;

			BoundingBox testBox;
		
			if (velocity.Z < 0)
			{
				testBox = new BoundingBox(
					new Vector3(
						Entity.BoundingBox.Min.X, 
						Entity.BoundingBox.Min.Y,
						Entity.BoundingBox.Min.Z + velocity.Z),  
					Entity.BoundingBox.Max);

				negative = true;
			}
			else
			{
				testBox = new BoundingBox(
					Entity.BoundingBox.Min,
					new Vector3(
						Entity.BoundingBox.Max.X,  
						Entity.BoundingBox.Max.Y,
						Entity.BoundingBox.Max.Z + velocity.Z)
					);

				negative = false;
			}

			float?            collisionExtent = null;

			for (int x = (int) (Math.Floor(testBox.Min.X)); x <= (int) (Math.Ceiling(testBox.Max.X)); x++)
			{
				for (int z = (int) (Math.Floor(testBox.Min.Z)); z <= (int) (Math.Ceiling(testBox.Max.Z)); z++)
				{
					for (int y = (int) (Math.Floor(testBox.Min.Y)); y <= (int) (Math.Ceiling(testBox.Max.Y)); y++)
					{
						var blockState = Entity.Level.GetBlockState(x, y, z);
						if (blockState?.Model == null || !blockState.Block.Solid)
							continue;

						var coords = new Vector3(x, y, z);
						
						foreach (var box in blockState.Model.GetBoundingBoxes(coords))
						{
							if (box.Max.Y <= testBox.Min.Y) continue;

							if (negative)
							{
								if (box.Max.Z <= testBox.Min.Z)
									continue;
								
								if (Entity.BoundingBox.Min.Z - box.Max.Z < 0)
									continue;
							}
							else
							{
								if (box.Min.X >= testBox.Max.X)
									continue;
								
								if (box.Min.Z - Entity.BoundingBox.Max.Z < 0)
									continue;
							}
							
							if (testBox.Intersects(box))
							{
								boxes.Add(box);

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
				double diff;

				if (negative)
					diff = -(Entity.BoundingBox.Min.Z - collisionExtent.Value);
				else
					diff = (collisionExtent.Value - Entity.BoundingBox.Max.Z);

				//velocity.Z = (float)diff;	
				result = (float) diff;

				return true;
			}

			return false;
		}

		private const float MaxJumpHeight = 0.55f;
	}
}