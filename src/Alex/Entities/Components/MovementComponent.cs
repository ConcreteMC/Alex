using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Common.Utils.Vectors;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Entities.Components
{
	public class MovementComponent : EntityComponent
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MovementComponent));
		public Vector3 Heading { get; private set; }
		
		/// <inheritdoc />
		public MovementComponent(Entity entity) : base(entity)
		{
			Heading = Vector3.Zero;
		}

		/// <inheritdoc />
		protected override void OnUpdate(float deltaTime)
		{
			var entity    = Entity;

			UpdateDistanceMoved(deltaTime);

			if ((_target == null || _from == null))
			{
				UpdateTarget();
				return;
			}

			if (_frameAccumulator >= TargetTime)
				return;
			
			_frameAccumulator += deltaTime;

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
		
		private float _distanceMoved = 0f, _lastDistanceMoved = 0f;
		public float TotalDistanceMoved { get; set; } = 0f;
		public float DistanceMoved
		{
			get => _distanceMoved;
			set
			{
				_distanceMoved = value;
			}
		}

		private float _verticalDistanceMoved = 0f, _lastVerticalDistanceMoved = 0f;
		public float VerticalDistanceMoved
		{
			get => _verticalDistanceMoved;
			set
			{
				_verticalDistanceMoved = value;
			}
		}

		public float VerticalSpeed { get; private set; } = 0f;
		
		private object _headingLock = new object();
		public void UpdateHeading(Vector3 heading)
		{
			lock (_headingLock)
			{
				Heading = heading.Transform(Entity.KnownPosition.GetDirectionMatrix(Entity.IsSwimming, true));
			}
		}
		
		public void MoveTo(PlayerLocation location, bool updateLook = true)
		{
			//var difference = Entity.KnownPosition.ToVector3() - location.ToVector3();
			//Move(difference);

			if (!updateLook)
			{
				location.Yaw = Entity.KnownPosition.Yaw;
				location.HeadYaw = Entity.KnownPosition.HeadYaw;
				location.Pitch = Entity.KnownPosition.Pitch;
			}
			
			Entity.KnownPosition = location;

			UpdateTarget();
		}

		public Vector3 Move(Vector3 amount)
		{
			var   oldPosition = Entity.KnownPosition.ToVector3();
			float offset      = 0f;

			//TODO: Fix position offset

			if (Entity.HasCollision)
			{
				var beforeAdjustment = new Vector3(amount.X, amount.Y, amount.Z);

				List<ColoredBoundingBox> boxes = new List<ColoredBoundingBox>();

				bool collideY = TestTerrainCollisionY(
					ref amount, out var yCollisionPoint, out var collisionY, boxes);

				if (collideY)
				{
					AdjustY(beforeAdjustment, yCollisionPoint, collisionY, ref amount);
				}

				bool collideX = TestTerrainCollisionX(ref amount, out var xCollisionPoint, out var collisionX, boxes);

				bool collideZ = TestTerrainCollisionZ(ref amount, out var zCollisionPoint, out var collisionZ, boxes);


				bool jumped = false;
				if ((collideX || collideZ) && Entity.FeetInWater)
				{
					Entity.CanSurface = true;
				}
				else
				{
					Entity.CanSurface = false;
				}
				/*
				 if (Entity.FeetInWater && !Entity.HeadInWater)
				{
					Entity.Jump();
				}
				 */
				bool jumpX = false;
				if (collideX)
				{
					if (!CheckJump(ref amount))
					{
						AdjustX(beforeAdjustment, xCollisionPoint, collisionX, ref amount);
						collideX = false;
					}
					else
					{
						jumpX = true;
					}

					//collideY = TestTerrainCollisionY(ref amount, out yCollisionPoint, out collisionY, boxes);
					//jumped = CheckJump(ref amount);
				}

				bool jumpZ = false;
				if (collideZ)
				{
					if (!CheckJump(ref amount))
					{
						AdjustZ(beforeAdjustment, zCollisionPoint, collisionZ, ref amount);
						collideZ = false;
					}
					else
					{
						jumpZ = true;
					}
					//collideY = TestTerrainCollisionY(ref amount, out yCollisionPoint, out collisionY, boxes);
					//jumped = CheckJump(ref amount);
				}

				//if (!CheckCorners(amount))
				//{
				//	amount = Vector3.Zero;
				//}

				if (boxes.Count > 0)
				{
					LastCollision = boxes.ToArray();
				}
			}

			Entity.KnownPosition += amount;
			Entity.KnownPosition.OnGround = DetectOnGround();
			
			UpdateTarget();
			
		/*	DistanceMoved +=
				MathF.Abs(Microsoft.Xna.Framework.Vector3.Distance(oldPosition * new Vector3(1f, 0f, 1f),
					Entity.KnownPosition.ToVector3() * new Vector3(1f, 0f, 1f)));
			
			VerticalDistanceMoved +=
				MathF.Abs(Microsoft.Xna.Framework.Vector3.Distance(oldPosition * new Vector3(0f, 1f, 0f),
					Entity.KnownPosition.ToVector3() * new Vector3(0f, 1f, 0f)));*/

			return amount;
		}

		/*private bool CheckCorners(Vector3 amount)
		{
			var modifiedPosition = Entity.KnownPosition + amount;
			var minY = MathF.Min(modifiedPosition.Y, Entity.KnownPosition.Y);
			var maxY = MathF.Max(modifiedPosition.Y, Entity.KnownPosition.Y);
			
			var entityBox = Entity.GetBoundingBox(modifiedPosition);
			var bottomCorners = entityBox.GetCorners().Where(x => x.Y < entityBox.Max.Y).ToArray();

			foreach (var corner in bottomCorners)
			{
				for (int y = (int) (Math.Floor(minY)); y <= (int) (Math.Ceiling(maxY)); y++)
				{
					var blockCoords = new BlockCoordinates(new PlayerLocation(corner.X, y, corner.Z));
					var blockState = Entity.Level.GetBlockState(blockCoords);

					if (!blockState.Block.Solid)
						continue;

					if (blockState.Block.GetBoundingBoxes(blockCoords).Any(x => x.Intersects(entityBox) && !(x.Max.Y - minY < 0.015f)))
						return false;
				}
			}

			return true;
		}*/
		
		private void AdjustY(Vector3 beforeAdjustment, Vector3 yCollisionPoint, float adjusted, ref Vector3 amount)
		{
			Entity.CollidedWithWorld(
				beforeAdjustment.Y < 0 ? Vector3.Down : Vector3.Up, yCollisionPoint, beforeAdjustment.Y);

			//if (adjusted < 0f)
			//	adjusted -= 0.01f;
			
			amount.Y = adjusted;

			Entity.Velocity = new Vector3(Entity.Velocity.X, 0f, Entity.Velocity.Z);
		}

		private void AdjustZ(Vector3 beforeAdjustment, Vector3 zCollisionPoint, float adjusted, ref Vector3 amount)
		{
			//amount.Z = collisionZ;

			Entity.CollidedWithWorld(
				beforeAdjustment.Z < 0 ? Vector3.Backward : Vector3.Forward, zCollisionPoint, beforeAdjustment.Z);

			var dir = (zCollisionPoint - Entity.KnownPosition);
			dir.Normalize();


			if (adjusted < 0f)
				adjusted -= 0.01f;

			amount.Z += adjusted;

			Entity.Velocity = new Vector3(Entity.Velocity.X, Entity.Velocity.Y, 0f);
		}

		private void AdjustX(Vector3 beforeAdjustment, Vector3 xCollisionPoint, float adjusted, ref Vector3 amount)
		{
			Entity.CollidedWithWorld(
				beforeAdjustment.X < 0 ? Vector3.Left : Vector3.Right, xCollisionPoint, beforeAdjustment.X);

			var dir = (xCollisionPoint - Entity.KnownPosition);
			dir.Normalize();

			if (adjusted < 0f)
				adjusted -= 0.01f;

			amount.X += adjusted;
			Entity.Velocity = new Vector3(0f, Entity.Velocity.Y, Entity.Velocity.Z);
		}

		public ColoredBoundingBox[] LastCollision { get; private set; } = new ColoredBoundingBox[0];
		
		private PlayerLocation _from;
		private PlayerLocation _target;
		public void UpdateTarget()
		{
			var target = Entity.KnownPosition;

			//if (!InterpolatedMovement)
			//{
			//	Entity.RenderLocation = target;
			//	return;
			//}
			
			var distance = Microsoft.Xna.Framework.Vector3.DistanceSquared(
				Entity.RenderLocation.ToVector3(), target.ToVector3());

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
			//velocity = velocity.Transform(Entity.KnownPosition.GetDirectionMatrix(false, true));
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

		private       float _frameAccumulator = 0f;
		private const float TargetTime        = 1f / 20f;
		
		public float MetersPerSecond { get; private set; } = 0f;
		private Vector3 _previousPosition = Vector3.Zero;

		private float _speedAccumulator = 0f;
		private void UpdateDistanceMoved(float deltaTime)
		{
			_speedAccumulator += deltaTime;
			var current = Entity.RenderLocation.ToVector3();
			var previous = _previousPosition;

			var horizontalDistance = MathF.Abs(
				Microsoft.Xna.Framework.Vector3.Distance(
					current * new Vector3(1f, 0f, 1f), previous * new Vector3(1f, 0f, 1f)));
			
			var verticalDistance = MathF.Abs(Microsoft.Xna.Framework.Vector3.Distance(
				current * new Vector3(0f, 1f, 0f), previous * new Vector3(0f, 1f, 0f)));

			if (horizontalDistance > 0f)
			{
				DistanceMoved += horizontalDistance;
				TotalDistanceMoved += horizontalDistance;
			}

			if (verticalDistance > 0f)
			{
				VerticalDistanceMoved += verticalDistance;
			}

			if (_speedAccumulator >= TargetTime)
			{
				var modifier = (1f / _speedAccumulator);
				
				var horizontalDist = _distanceMoved - _lastDistanceMoved;
				_lastDistanceMoved = _distanceMoved;

				if (horizontalDist <= 0.001f)
				{
					horizontalDist = 0f;
					_lastDistanceMoved = _distanceMoved = 0f;
				}

				MetersPerSecond = (float) (horizontalDist * modifier);
				
				var verticalDistanceMoved = _verticalDistanceMoved - _lastVerticalDistanceMoved;
				_lastVerticalDistanceMoved = _verticalDistanceMoved;

				if (verticalDistanceMoved <= 0.001f)
				{
					verticalDistanceMoved = 0f;
					_lastVerticalDistanceMoved = _verticalDistanceMoved = 0f;
				}

				VerticalSpeed = (float) (verticalDistanceMoved * modifier);
				
				_speedAccumulator = 0f;
			}

			_previousPosition = current;
		}

		private bool CheckJump(ref Vector3 amount)
		{
			var   canJump = false;
			//float yTarget = amount.Y;
			if (Entity.KnownPosition.OnGround && MathF.Abs(Entity.Velocity.Y) < 0.001f)
			{
				canJump = true;
				var adjusted     = Entity.GetBoundingBox(Entity.KnownPosition + amount);
				var intersecting = GetIntersecting(Entity.Level, adjusted);
				var targetY      = 0f;

				//if (!PhysicsManager.GetIntersecting(Entity.Level, adjusted).Any(bb => bb.Max.Y >= adjusted.Min.Y && bb.Min.Y <= adjusted.Max.Y))
				foreach (var box in intersecting)
				{
					var yDifference = box.Max.Y - adjusted.Min.Y;

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
					//var originalY = amount.Y;
					//yTarget = targetY;
					//var a = intersecting.
					adjusted     = Entity.GetBoundingBox(Entity.KnownPosition + new Vector3(amount.X, targetY, amount.Z));

					if (GetIntersecting(Entity.Level, adjusted).Any(
						bb => bb.Max.Y > adjusted.Min.Y && bb.Min.Y <= adjusted.Max.Y))
					{
						canJump = false;
						//yTarget = originalY;
					}
				}
				else
				{
					canJump = false;
				}

				if (canJump)
				{
					amount.Y = targetY;
				}
			}

			return canJump;
		}
		
		private bool DetectOnGround()
		{
			var entityBoundingBox =
				Entity.BoundingBox;
			//entityBoundingBox.Inflate(0.01f);
			
			var offset = 0f;

			if (entityBoundingBox.Min.Y % 1 < 0.05f)
			{
				offset = -1f;
			}

			bool foundGround = false;

			var minX = entityBoundingBox.Min.X;
			//if (minX < 0f)
			//	minX -= 1;
			
			var minZ = entityBoundingBox.Min.Z;
			//if (minZ < 0f)
			//	minZ -= 1;
			
			var maxX = entityBoundingBox.Max.X;
			var maxZ = entityBoundingBox.Max.Z;
			
			var y = (int)Math.Floor(entityBoundingBox.Min.Y + offset);
			for (int x = (int) (Math.Floor(minX)); x <= (int) (Math.Ceiling(maxX)); x++)
			{
				for (int z = (int) (Math.Floor(minZ)); z <= (int) (Math.Ceiling(maxZ)); z++)
				{
					var blockState = Entity.Level.GetBlockState(x, y, z);
					if (!blockState.Block.Solid)
						continue;
					
					var coords = new Vector3(x, y, z);
					foreach (var box in blockState.Block.GetBoundingBoxes(coords).OrderBy(x => x.Max.Y))
					{
						var yDifference = MathF.Abs(entityBoundingBox.Min.Y - box.Max.Y); // <= 0.01f

						if (yDifference > 0.015f)
							continue;

						if (box.Intersects(entityBoundingBox))
						{
							return true;
						}

						//return true;
					}
				}
			}

			/*foreach (var corner in entityBoundingBox.GetCorners()
			   .Where(x => Math.Abs(x.Y - entityBoundingBox.Min.Y) < 0.001f))
			{

				var blockcoords = new BlockCoordinates(
					new PlayerLocation(corner.X, Math.Floor(corner.Y + offset), corner.Z));

				var block = Entity.Level.GetBlockState(blockcoords.X, blockcoords.Y, blockcoords.Z);

				if (!block.Block.Solid)
					continue;

				foreach (var box in block.Block.GetBoundingBoxes(blockcoords).OrderBy(x => x.Max.Y))
				{
					var yDifference = MathF.Abs(entityBoundingBox.Min.Y - box.Max.Y); // <= 0.01f

					if (yDifference > 0.015f)
						continue;

					if (box.Contains(corner) == ContainmentType.Contains)
					{
						return true;
					}

					//return true;
				}
			}*/

			return foundGround;
		}
		
		private void AdjustVelocityForCollision(ref Vector3 velocity, BoundingBox problem)
		{
			if (velocity.X < 0f)
				velocity.X = -(Entity.BoundingBox.Min.X - problem.Max.X);
			else
				velocity.X = (problem.Min.X - Entity.BoundingBox.Max.X);

			if (velocity.Y < 0)
				velocity.Y = -(Entity.BoundingBox.Min.Y - problem.Max.Y);
			if (velocity.Y > 0)
				velocity.Y = Entity.BoundingBox.Max.Y - problem.Min.Y;
			
			if (velocity.Z < 0f)
				velocity.Z = -(Entity.BoundingBox.Min.Z - problem.Max.Z);
			else
				velocity.Z = (problem.Min.Z - Entity.BoundingBox.Max.Z);
		}
		
		private bool TestTerrainCollisionY(ref Vector3 velocity, out Vector3 collisionPoint, out float result, List<ColoredBoundingBox> boxes)
		{
			collisionPoint = Vector3.Zero;
			result = velocity.Y;

			if (MathF.Abs(velocity.Y) < 0.0001f)
				return false;

			bool negative;

			var entityBox = Entity.BoundingBox;
			BoundingBox testBox;
			if (velocity.Y < 0)
			{
				testBox = new BoundingBox(
					new Vector3(
						entityBox.Min.X,  entityBox.Min.Y + velocity.Y,
						entityBox.Min.Z),  entityBox.Max);

				negative = true;
			}
			else
			{
				testBox = new BoundingBox(
					entityBox.Min,
					new Vector3(
						entityBox.Max.X,  entityBox.Max.Y + velocity.Y,
						entityBox.Max.Z));

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
						if (!blockState.Block.Solid)
							continue;

						//var chunk = Entity.Level.GetChunk(new BlockCoordinates(x,y,z));

						var coords = new Vector3(x, y, z);
						
						foreach (var box in blockState.Block.GetBoundingBoxes(coords))
						{
							if (negative)
							{
								if (entityBox.Min.Y - box.Max.Y < 0)
									continue;
							}
							else
							{
								if (box.Min.Y - entityBox.Max.Y < 0)
									continue;
							}
							
							if (testBox.Intersects(box))
							{
								boxes.Add(new ColoredBoundingBox(box, Color.Green));
								
								if (negative)
								{
									if ((collisionExtent == null || collisionExtent.Value < box.Max.Y))
									{
										collisionExtent = box.Max.Y;
										collisionPoint = coords;
									}
								}
								else
								{
									if ((collisionExtent == null || collisionExtent.Value > box.Min.Y))
									{
										collisionExtent = box.Min.Y;
										collisionPoint = coords;
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
					diff = extent - entityBox.Min.Y;
				else
					diff = extent - entityBox.Max.Y;

				result = (float)diff;	
				
				return true;
			}
			
			return false;
		}

		private bool TestTerrainCollisionX(ref Vector3 velocity, out Vector3 collisionPoint, out float result, List<ColoredBoundingBox> boxes)
		{
			result = velocity.X;
			collisionPoint = Vector3.Zero;

			//if (velocity.X == 0)
			//	return false;
			
			bool negative;

			var entityBox = Entity.BoundingBox;
			BoundingBox testBox;

			if (velocity.X < 0)
			{
				testBox = new BoundingBox(
					new Vector3(
						entityBox.Min.X + velocity.X, 
						entityBox.Min.Y,
						entityBox.Min.Z),
					entityBox.Max);

				negative = true;
			}
			else
			{
				testBox = new BoundingBox(
					entityBox.Min,
					new Vector3(
						entityBox.Max.X + velocity.X, 
						entityBox.Max.Y,
						entityBox.Max.Z));

				negative = false;
			}

			var minX = testBox.Min.X;
			//if (minX < 0f)
			//	minX -= 1;
			
			var minZ = testBox.Min.Z;
			//if (minZ < 0f)
			//	minZ -= 1;
			
			var maxX = testBox.Max.X;
			var maxZ = testBox.Max.Z;
			
			float?            collisionExtent = null;

			for (int x = (int) (Math.Floor(minX)); x <= (int) (Math.Ceiling(maxX)); x++)
			{
				for (int z = (int) (Math.Floor(minZ)); z <= (int) (Math.Ceiling(maxZ)); z++)
				{
					for (int y = (int) (Math.Floor(testBox.Min.Y)); y <= (int) (Math.Ceiling(testBox.Max.Y)); y++)
					{
						var blockState = Entity.Level.GetBlockState(x, y, z);
						if (!blockState.Block.Solid)
							continue;

						var coords = new Vector3(x, y, z);
						
						foreach (var box in blockState.Block.GetBoundingBoxes(coords))
						{
							if (box.Max.Y <= testBox.Min.Y) continue;
							
							if (negative)
							{
								if (box.Max.X <= minX)
									continue;
								
								if (entityBox.Min.X - box.Max.X < 0)
									continue;
							}
							else
							{
								if (box.Min.Z >= maxZ)
									continue;
								
								if (box.Min.X - entityBox.Max.X < 0)
									continue;
							}
							
							if (testBox.Intersects(box))
							{
								boxes.Add(new ColoredBoundingBox(box, Color.Red));

								if (negative)
								{
									if ((collisionExtent == null || collisionExtent.Value < box.Max.X))
									{
										collisionExtent = box.Max.X;
										collisionPoint = coords;
									}
								}
								else
								{
									if ((collisionExtent == null || collisionExtent.Value > box.Min.X))
									{
										collisionExtent = box.Min.X;
										collisionPoint = coords;
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
				{
					diff = (collisionExtent.Value - minX);
				}
				else
				{
					diff = (collisionExtent.Value - maxX);
				}

				result = (float) diff;

				//if (Entity is Player p)
				//	Log.Debug($"ColX, Distance={diff}, X={(negative ? minX : maxX)} PointOfCollision={collisionExtent.Value} (negative: {negative})");
				
				return true;
			}

			return false;
		}

		private bool TestTerrainCollisionZ(ref Vector3 velocity, out Vector3 collisionPoint, out float result, List<ColoredBoundingBox> boxes)
		{
			result = velocity.Z;
			collisionPoint = Vector3.Zero;

			//if (velocity.Z == 0)
			//	return false;

			bool negative;

			var entityBox = Entity.BoundingBox;
			BoundingBox testBox;
		
			if (velocity.Z < 0)
			{
				testBox = new BoundingBox(
					new Vector3(
						entityBox.Min.X, 
						entityBox.Min.Y,
						entityBox.Min.Z + velocity.Z),  
					entityBox.Max);

				negative = true;
			}
			else
			{
				testBox = new BoundingBox(
					entityBox.Min,
					new Vector3(
						entityBox.Max.X,  
						entityBox.Max.Y,
						entityBox.Max.Z + velocity.Z)
					);

				negative = false;
			}

			var minX = testBox.Min.X;
			//if (minX < 0f)
			//	minX -= 1;
			
			var minZ = testBox.Min.Z;
			//if (minZ < 0f)
			//	minZ -= 1;
			
			var maxX = testBox.Max.X;
			var maxZ = testBox.Max.Z;
			
			float?            collisionExtent = null;

			for (int x = (int) (Math.Floor(minX)); x <= (int) (Math.Ceiling(maxX)); x++)
			{
				for (int z = (int) (Math.Floor(minZ)); z <= (int) (Math.Ceiling(maxZ)); z++)
				{
					for (int y = (int) (Math.Floor(testBox.Min.Y)); y <= (int) (Math.Ceiling(testBox.Max.Y)); y++)
					{
						var blockState = Entity.Level.GetBlockState(x, y, z);
						if (!blockState.Block.Solid)
							continue;

						var coords = new Vector3(x, y, z);
						
						foreach (var box in blockState.Block.GetBoundingBoxes(coords))
						{
							if (box.Max.Y <= testBox.Min.Y) continue;

							if (negative)
							{
								if (box.Max.Z <= minZ)
									continue;
								
								if (entityBox.Min.Z - box.Max.Z < 0)
									continue;
							}
							else
							{
								if (box.Min.X >= maxX)
									continue;
								
								if (box.Min.Z - entityBox.Max.Z < 0)
									continue;
							}
							
							if (testBox.Intersects(box))
							{
								boxes.Add(new ColoredBoundingBox(box, Color.Blue));

								if (negative)
								{
									if ((collisionExtent == null || collisionExtent.Value < box.Max.Z))
									{
										collisionExtent = box.Max.Z;
										collisionPoint = coords;
									}
								}
								else
								{
									if ((collisionExtent == null || collisionExtent.Value > box.Min.Z))
									{
										collisionExtent = box.Min.Z;
										collisionPoint = coords;
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
					diff = (collisionExtent.Value - minZ);
				else
					diff = (collisionExtent.Value - maxZ);

				//velocity.Z = (float)diff;	
				result = (float) diff;

				//if (Entity is Player p)
				//	Log.Debug($"ColZ, Distance={diff}, Z={(negative ? minZ : maxZ)} PointOfCollision={collisionExtent.Value} (negative: {negative})");
				
				return true;
			}

			return false;
		}
		
		private static IEnumerable<BoundingBox> GetIntersecting(World world, BoundingBox box)
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

		private const float MaxJumpHeight = 0.55f;
	}
}