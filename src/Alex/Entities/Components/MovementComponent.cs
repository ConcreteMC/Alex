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
			private set
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
				Heading = heading;
				//Heading = heading.Transform(Entity.KnownPosition.GetDirectionMatrix(Entity.IsSwimming, true));
			}
		}
		
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
		
		private       float _frameAccumulator = 0f;
		private const float TargetTime        = 1f / 20f;
		
		public float MetersPerSecond { get; private set; } = 0f;
		private Vector3 _previousPosition = Vector3.Zero;

		private float _speedAccumulator = 0f;
		private void UpdateDistanceMoved(float deltaTime)
		{
			_speedAccumulator += deltaTime;
			
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

				var mps = MetersPerSecond;
				mps += horizontalDist * modifier;
				mps /= 2f;
				
				MetersPerSecond = mps;
				
				var verticalDistanceMoved = _verticalDistanceMoved - _lastVerticalDistanceMoved;
				_lastVerticalDistanceMoved = _verticalDistanceMoved;

				if (verticalDistanceMoved <= 0.001f)
				{
					verticalDistanceMoved = 0f;
					_lastVerticalDistanceMoved = _verticalDistanceMoved = 0f;
				}

				var vmps = VerticalSpeed;
				vmps += verticalDistanceMoved * modifier;
				vmps /= 2f;
				
				VerticalSpeed = vmps;
				
				_speedAccumulator = 0f;
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
		
		public void MoveTo(PlayerLocation location, bool updateLook = true)
		{
			if (!updateLook)
			{
				location.Yaw = Entity.KnownPosition.Yaw;
				location.HeadYaw = Entity.KnownPosition.HeadYaw;
				location.Pitch = Entity.KnownPosition.Pitch;
			}
			
			var difference = location.ToVector3() - Entity.KnownPosition.ToVector3();
			MovedBy(difference);
			
			Entity.KnownPosition = location;

			UpdateTarget();
		}

		private void MovedBy(Vector3 amount)
		{
			var horizontalDistance = MathF.Abs(
				(amount * new Vector3(1f, 0f, 1f)).Length());
			
			var verticalDistance = MathF.Abs((amount * new Vector3(0f, 1f, 0f)).Length());

			if (horizontalDistance > 0.001f)
			{
				DistanceMoved += horizontalDistance;
				TotalDistanceMoved += horizontalDistance;
			}

			if (verticalDistance > 0.001f)
			{
				VerticalDistanceMoved += verticalDistance;
			}
		}

		public Vector3 Move(Vector3 amount)
		{
			if (Entity.HasCollision)
			{
				var beforeAdjustment = new Vector3(amount.X, amount.Y, amount.Z);

				List<ColoredBoundingBox> boxes = new List<ColoredBoundingBox>();
				
				bool collided = false;

				bool collideY = CheckY(ref amount, false, ref boxes);

				bool collideX = CheckX(ref amount, false, ref boxes);
				
				bool collideZ = CheckZ(ref amount, false, ref boxes);

				if (!collideX && CheckX(ref amount, true, ref boxes))
				{
					collideX = true;
				}

				if (!collideZ && CheckZ(ref amount, true, ref boxes))
				{
					collideZ = true;
				}

				collided = collideX || collideZ;

				if (collided && Entity.FeetInWater && !Entity.HeadInWater)
				{
					Entity.CanSurface = true;
				}
				else
				{
					Entity.CanSurface = false;
				}

				if (boxes.Count > 0)
				{
					LastCollision = boxes.ToArray();
				}
			}
			
			Entity.KnownPosition += amount;
			Entity.KnownPosition.OnGround = DetectOnGround();
			
			MovedBy(amount);
			UpdateTarget();

			return amount;
		}

		private bool CheckY(ref Vector3 amount, bool checkOther, ref List<ColoredBoundingBox> boxes)
		{
			var beforeAdjustment = amount.Y;

			if (!TestTerrainCollisionY(ref amount, out var yCollisionPoint, out var collisionY, boxes))
				return false;
			
			Entity.CollidedWithWorld(
				beforeAdjustment < 0 ? Vector3.Down : Vector3.Up, yCollisionPoint, beforeAdjustment);

			amount.Y = collisionY;

			Entity.Velocity = new Vector3(Entity.Velocity.X, 0, Entity.Velocity.Z);

			return true;
		}

		private bool CheckX(ref Vector3 amount, bool checkOther, ref List<ColoredBoundingBox> boxes)
		{
			if (!TestTerrainCollisionX(amount, out _, out var collisionX, boxes, checkOther)) 
				return false;

			if (CheckJump(amount, out float yValue))
			{
				amount.Y = yValue;
			}
			else
			{
				if (collisionX < 0f)
					collisionX -= 0.005f;

				amount.X += collisionX;
				Entity.Velocity = new Vector3(0, Entity.Velocity.Y, Entity.Velocity.Z);
			}

			return true;
		}
		
		private bool CheckZ(ref Vector3 amount, bool checkOther, ref List<ColoredBoundingBox> boxes)
		{
			if (!TestTerrainCollisionZ(amount, out _, out var collisionZ, boxes, checkOther)) 
				return false;

			if (CheckJump(amount, out float yValue))
			{
				amount.Y = yValue;
			}
			else
			{
				if (collisionZ < 0f)
					collisionZ -= 0.005f;

				amount.Z += collisionZ;
				Entity.Velocity = new Vector3(Entity.Velocity.X, Entity.Velocity.Y, 0f);
			}

			return true;
		}

		public ColoredBoundingBox[] LastCollision { get; private set; } = new ColoredBoundingBox[0];

		private bool CheckJump(Vector3 amount, out float yValue)
		{
			yValue = amount.Y;
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
					yValue = targetY;
					//amount.Y = targetY;
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

			return foundGround;
		}

		private bool TestTerrainCollisionY(ref Vector3 velocity, out Vector3 collisionPoint, out float result, List<ColoredBoundingBox> boxes)
		{
			collisionPoint = Vector3.Zero;
			result = velocity.Y;

			//if (MathF.Abs(velocity.Y) < 0.0001f)
			//	return false;

			bool negative;

			var entityBox = Entity.BoundingBox;
			BoundingBox testBox;
			if (velocity.Y < 0)
			{
				testBox = new BoundingBox(
					new Vector3(
						entityBox.Min.X,  
						entityBox.Min.Y + velocity.Y,
						entityBox.Min.Z),  entityBox.Max);

				negative = true;
			}
			else
			{
				testBox = new BoundingBox(
					entityBox.Min,
					new Vector3(
						entityBox.Max.X,  
						entityBox.Max.Y + velocity.Y,
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

				double diff;
				if (negative)
					diff = extent - entityBox.Min.Y;
				else
					diff = extent - entityBox.Max.Y;

				result = (float)diff;	
				
				return true;
			}
			
			return false;
		}

		private bool TestTerrainCollisionX(Vector3 velocity, out Vector3 collisionPoint, out float result, List<ColoredBoundingBox> boxes, bool includeOther)
		{
			result = velocity.X;
			collisionPoint = Vector3.Zero;

			bool negative;

			var entityBox = Entity.BoundingBox;
			BoundingBox testBox;

			Vector3 min = new Vector3(entityBox.Min.X, entityBox.Min.Y, entityBox.Min.Z);
			Vector3 max = new Vector3(entityBox.Max.X, entityBox.Max.Y, entityBox.Max.Z);
			if (velocity.X < 0)
			{
				min.X += velocity.X;
				negative = true;
			}
			else
			{
				max.X += velocity.X;
				negative = false;
			}

			if (includeOther)
			{
				if (velocity.Z < 0)
				{
					min.Z += velocity.Z;
				}
				else
				{
					max.Z += velocity.Z;
				}

				if (velocity.Y < 0)
				{
					min.Y += velocity.Y;
				}
				else
				{
					max.Y += velocity.Y;
				}
			}

			testBox = new BoundingBox(min, max);

			var minX = testBox.Min.X;
			var minZ = testBox.Min.Z;

			var maxX = testBox.Max.X;
			var maxZ = testBox.Max.Z;

			var minY = testBox.Min.Y;
			var maxY = testBox.Max.Y;
			
			float?            collisionExtent = null;

			for (int x = (int) (Math.Floor(minX)); x <= (int) (Math.Ceiling(maxX)); x++)
			{
				for (int z = (int) (Math.Floor(minZ)); z <= (int) (Math.Ceiling(maxZ)); z++)
				{
					for (int y = (int) (Math.Floor(minY)); y <= (int) (Math.Ceiling(maxY)); y++)
					{
						var blockState = Entity.Level.GetBlockState(x, y, z);
						if (!blockState.Block.Solid)
							continue;

						var coords = new Vector3(x, y, z);
						
						foreach (var box in blockState.Block.GetBoundingBoxes(coords))
						{
							if (box.Max.Y <= minY) continue;
							
							if (negative)
							{
								if (box.Max.X <= minX)
									continue;
								
								if (entityBox.Min.X - box.Max.X < 0)
									continue;
							}
							else
							{
								if (box.Min.X >= maxX)
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
					diff = (collisionExtent.Value - minX) + 0.01f;
				}
				else
				{
					diff = (collisionExtent.Value - maxX);
				}

				result = (float) diff;

			//	if (Entity is Player p)
				//	Log.Debug($"ColX, Distance={diff}, X={(negative ? minX : maxX)} PointOfCollision={collisionExtent.Value} (negative: {negative})");
				
				return true;
			}

			return false;
		}

		private bool TestTerrainCollisionZ(Vector3 velocity, out Vector3 collisionPoint, out float result, List<ColoredBoundingBox> boxes, bool includeOther)
		{
			result = velocity.Z; 
			collisionPoint = Vector3.Zero;

			bool negative;

			var entityBox = Entity.BoundingBox;
			BoundingBox testBox;
		
			Vector3 min = new Vector3(entityBox.Min.X, entityBox.Min.Y, entityBox.Min.Z);
			Vector3 max = new Vector3(entityBox.Max.X, entityBox.Max.Y, entityBox.Max.Z);
			if (velocity.Z < 0)
			{
				min.Z += velocity.Z;
				negative = true;
			}
			else
			{
				max.Z += velocity.Z;
				negative = false;
			}

			if (includeOther)
			{
				if (velocity.X < 0)
				{
					min.X += velocity.X;
				}
				else
				{
					max.X += velocity.X;
				}

				if (velocity.Y < 0)
				{
					min.Y += velocity.Y;
				}
				else
				{
					max.Y += velocity.Y;
				}
			}

			testBox = new BoundingBox(min, max);

			var minX = testBox.Min.X;
			var minZ = testBox.Min.Z;

			var maxX = testBox.Max.X;
			var maxZ = testBox.Max.Z;

			var minY = testBox.Min.Y;
			var maxY = testBox.Max.Y;
			
			float?            collisionExtent = null;

			for (int x = (int) (Math.Floor(minX)); x <= (int) (Math.Ceiling(maxX)); x++)
			{
				for (int z = (int) (Math.Floor(minZ)); z <= (int) (Math.Ceiling(maxZ)); z++)
				{
					for (int y = (int) (Math.Floor(minY)); y <= (int) (Math.Ceiling(maxY)); y++)
					{
						var blockState = Entity.Level.GetBlockState(x, y, z);
						if (!blockState.Block.Solid)
							continue;

						var coords = new Vector3(x, y, z);
						
						foreach (var box in blockState.Block.GetBoundingBoxes(coords))
						{
							if (box.Max.Y <= minY) continue;

							if (negative)
							{
								if (box.Max.Z <= minZ)
									continue;
								
								if (entityBox.Min.Z - box.Max.Z < 0)
									continue;
							}
							else
							{
								if (box.Min.Z >= maxZ)
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
				var cp = collisionExtent.Value;
				
				double diff;

				if (negative)
					diff = (cp - minZ) + 0.01f;
				else
					diff = (cp - maxZ);

				//velocity.Z = (float)diff;	
				result = (float) diff;

			//	if (Entity is Player p)
			//		Log.Debug($"ColZ, Distance={diff}, MinZ={(minZ)} MaxZ={maxZ} PointOfCollision={cp} (negative: {negative})");
				
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
				var coords = new Vector3(x, y, z);

				var block = world.GetBlockState(x,y,z);

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