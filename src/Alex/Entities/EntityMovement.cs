using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Alex.Api;
using Alex.API;
using Alex.API.Utils;
using Alex.API.Utils.Vectors;
using Alex.API.World;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Entities
{
	public class EntityMovement
	{
		public Entity  Entity  { get; }
		public Vector3 Heading { get; private set; }
		
		public EntityMovement(Entity entity)
		{
			Entity = entity;
			Heading = Vector3.Zero;
		}
		
		private Stopwatch _previousUpdate = Stopwatch.StartNew();
		private float _distanceMoved = 0f, _lastDistanceMoved = 0f;
		public float DistanceMoved
		{
			get => _distanceMoved;
			set
			{
				if (float.IsNaN(value) || float.IsInfinity(value))
					return;
				var mvt = value;

				_distanceMoved = value;
				
				//_speedAccumulator += frameTime;
				var distanceMoved = mvt - _lastDistanceMoved;
				
				if (distanceMoved <= 0.0005f && _previousUpdate.ElapsedMilliseconds < 50)
					return;
				
				_lastDistanceMoved = _distanceMoved;
				//RawSpeed = (float) (distanceMoved);
				//if (_speedAccumulator >= TargetTime)
				{
					//DistanceMoved = 0;

					var difference = _previousUpdate.Elapsed;
					//BlocksPerTick = (float) (distanceMoved * (TimeSpan.FromMilliseconds(50) / difference));// * (_speedAccumulator / TargetTime);
					MetersPerSecond = (float) (distanceMoved * (TimeSpan.FromSeconds(1) / difference));
				
					_previousUpdate.Restart();
					//_previousUpdate = DateTime.UtcNow;
					//PreviousUpdate
					//CurrentSpeed = (float) (distanceMoved * (TimeSpan.FromSeconds(1) / (DateTime.UtcNow - _previousUpdate)));
				
				}
			}
		}

		private Stopwatch _previousVerticalUpdate = Stopwatch.StartNew();
		private float _verticalDistanceMoved = 0f, _lastVerticalDistanceMoved = 0f;
		public float VerticalDistanceMoved
		{
			get => _verticalDistanceMoved;
			set
			{
				if (float.IsNaN(value) || float.IsInfinity(value))
					return;
				
				var mvt = value;
				_verticalDistanceMoved = value;

				//_speedAccumulator += frameTime;
				var distanceMoved = mvt - _lastVerticalDistanceMoved;
				var difference = _previousVerticalUpdate.Elapsed;
				if (distanceMoved <= 0.0005f && _previousVerticalUpdate.ElapsedMilliseconds < 50)
					return;
				
				_lastVerticalDistanceMoved = _verticalDistanceMoved;
				VerticalSpeed = (float) (distanceMoved * (TimeSpan.FromSeconds(1) / difference));
				_previousVerticalUpdate.Restart();
			}
		}

		public float VerticalSpeed { get; private set; } = 0f;
		
		private object _headingLock = new object();
		public void UpdateHeading(Vector3 heading)
		{
			lock (_headingLock)
			{
				Heading = heading.Transform(Entity.KnownPosition.GetDirectionMatrix(false, true));
			}
		}
		
		public void MoveTo(PlayerLocation location, bool updateLook = true)
		{
			DistanceMoved += MathF.Abs(Microsoft.Xna.Framework.Vector3.Distance(
				Entity.KnownPosition.ToVector3() * new Vector3(1f, 0f, 1f), location.ToVector3() * new Vector3(1f, 0f, 1f)));
			
			VerticalDistanceMoved += MathF.Abs(Microsoft.Xna.Framework.Vector3.Distance(
				Entity.KnownPosition.ToVector3() * new Vector3(0f, 1f, 0f), location.ToVector3() * new Vector3(0f, 1f, 0f)));
			
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

				bool collideX = TestTerrainCollisionX(
					ref amount, out var xCollisionPoint, out var collisionX, boxes);

				bool collideZ = TestTerrainCollisionZ(
					ref amount, out var zCollisionPoint, out var collisionZ, boxes);

				bool jumped = CheckJump(ref amount);

				//if (canJump)
				//{
				//	amount.Y = yTarget;
				//}
				//if (collideY)
					//canJump = false;
				
				if (!jumped && collideX)
				{
					AdjustX(beforeAdjustment, xCollisionPoint, collisionX, ref amount);
					collideX = false;
					
					//collideY = TestTerrainCollisionY(ref amount, out yCollisionPoint, out collisionY, boxes);
					jumped = CheckJump(ref amount);
				}
				
				//if (canJump)
				//{
				//	amount.Y = yTarget;
				//}

				if (!jumped && collideZ)
				{
					AdjustZ(beforeAdjustment, zCollisionPoint, collisionZ, ref amount);
					collideZ = false;
					
					//collideY = TestTerrainCollisionY(ref amount, out yCollisionPoint, out collisionY, boxes);
					jumped = CheckJump(ref amount);
				}

				//if (canJump)
				//{
				//	amount.Y = yTarget;
				//}
				
				//if (!canJump)
				{
					/*if (collideX)
					{
						AdjustX(beforeAdjustment, xCollisionPoint, collisionX, ref amount);
					}

					if (collideZ)
					{
						AdjustZ(beforeAdjustment, zCollisionPoint, collisionZ, ref amount);
					}*/
				}
				
				if (boxes.Count > 0)
				{
					LastCollision = boxes.ToArray();
				}
			}

			Entity.KnownPosition += amount;
			Entity.KnownPosition.OnGround = DetectOnGround();
			
			UpdateTarget();
			
			DistanceMoved +=
				MathF.Abs(Microsoft.Xna.Framework.Vector3.Distance(oldPosition * new Vector3(1f, 0f, 1f),
					Entity.KnownPosition.ToVector3() * new Vector3(1f, 0f, 1f)));
			
			VerticalDistanceMoved +=
				MathF.Abs(Microsoft.Xna.Framework.Vector3.Distance(oldPosition * new Vector3(0f, 1f, 0f),
					Entity.KnownPosition.ToVector3() * new Vector3(0f, 1f, 0f)));

			return amount;
		}

		private void AdjustY(Vector3 beforeAdjustment, Vector3 yCollisionPoint, float adjusted, ref Vector3 amount)
		{
			Entity.CollidedWithWorld(
				beforeAdjustment.Y < 0 ? Vector3.Down : Vector3.Up, yCollisionPoint, beforeAdjustment.Y);

			amount.Y = adjusted;

			Entity.Velocity = new Vector3(Entity.Velocity.X, 0f, Entity.Velocity.Z);
		}

		private void AdjustZ(Vector3 beforeAdjustment, Vector3 zCollisionPoint, float adjusted, ref Vector3 amount)
		{
			//amount.Z = collisionZ;

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
				amount.Z = adjusted;

				Entity.Velocity = new Vector3(Entity.Velocity.X, Entity.Velocity.Y, 0f);
			}
		}
		
		private void AdjustX(Vector3 beforeAdjustment, Vector3 xCollisionPoint, float adjusted, ref Vector3 amount)
		{
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
				amount.X = adjusted;

				Entity.Velocity = new Vector3(0f, Entity.Velocity.Y, Entity.Velocity.Z);
			}
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
		
		public float MetersPerSecond { get; private set; } = 0f;
		//public float BlocksPerTick { get; private set; } = 0f;
		//public float Speed { get; private set; } = 0f;
		public void Update(GameTime gt)
		{
			var frameTime = (float) gt.ElapsedGameTime.TotalSeconds; // / 50;
			var entity    = Entity;

			if ((_target == null || _from == null))
			{
				UpdateTarget();
				return;
			}

			if (_frameAccumulator >= TargetTime)
				return;
			
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

		private bool CheckJump(ref Vector3 amount)
		{
			var   canJump = false;
			//float yTarget = amount.Y;
			if (Entity.KnownPosition.OnGround && MathF.Abs(Entity.Velocity.Y) < 0.001f)
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
					//var originalY = amount.Y;
					//yTarget = targetY;
					//var a = intersecting.
					adjusted     = Entity.GetBoundingBox(Entity.KnownPosition + new Vector3(amount.X, targetY, amount.Z));

					if (PhysicsManager.GetIntersecting(Entity.Level, adjusted).Any(
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
			}

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
		
		private BoundingBox GetAABBVelocityBox(Vector3 velocity)
		{
			var bound = Entity.BoundingBox;
			
			var min = new Vector3(
				Math.Min(bound.Min.X, bound.Min.X + velocity.X),
				Math.Min(bound.Min.Y, bound.Min.Y + velocity.Y),
				Math.Min(bound.Min.Z, bound.Min.Z + velocity.Z)
			);
			var max = new Vector3(
				Math.Max(bound.Max.X, bound.Max.X + velocity.X),
				Math.Max(bound.Max.Y, bound.Max.Y + velocity.Y),
				Math.Max(bound.Max.Z, bound.Max.Z + velocity.Z)
			);
			return new BoundingBox(min, max);
		}
		
		private bool TestTerrainCollisionCylinder(Vector3 velocity, out Vector3 adjustedVelocity, List<ColoredBoundingBox> boxes)
		{
			adjustedVelocity = velocity;
			
			var testBox = GetAABBVelocityBox(velocity);
			
			var testCylinder = new BoundingCylinder(testBox.Min, testBox.Max,
				Entity.Width / 2f);

			bool collision = false;
			for (int x = (int)(Math.Floor(testBox.Min.X)); x <= (int)(Math.Ceiling(testBox.Max.X)); x++)
			{
				for (int z = (int)(Math.Floor(testBox.Min.Z)); z <= (int)(Math.Ceiling(testBox.Max.Z)); z++)
				{
					for (int y = (int)(Math.Floor(testBox.Min.Y)); y <= (int)(Math.Ceiling(testBox.Max.Y)); y++)
					{
						var blockState = Entity.Level.GetBlockState(x, y, z);
						if (!blockState.Block.Solid)
							continue;

						var coords = new Vector3(x, y, z);

						foreach (var box in blockState.Block.GetBoundingBoxes(coords))
						{
							if (testCylinder.Intersects(box))
							{
								if (testBox.Intersects(box))
								{
									if (boxes != null)
										boxes.Add(new ColoredBoundingBox(box, Color.Orange));
									
									collision = true;
									
									AdjustVelocityForCollision(ref velocity, box);
									
									testBox = GetAABBVelocityBox(velocity); 
									testCylinder = new BoundingCylinder(
										testBox.Min, testBox.Max,
										Entity.Width / 2f);

									adjustedVelocity = velocity;
								}
							}
						}
					}
				}
			}
			
			return collision;
		}

		private bool TestTerrainCollisionY(ref Vector3 velocity, out Vector3 collisionPoint, out float result, List<ColoredBoundingBox> boxes)
		{
			collisionPoint = Vector3.Zero;
			result = velocity.Y;

			if (MathF.Abs(velocity.Y) < 0.0001f)
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
						if (!blockState.Block.Solid)
							continue;

						var chunk = Entity.Level.GetChunk(new BlockCoordinates(x,y,z));

						var coords = new Vector3(x, y, z);
						
						foreach (var box in blockState.Block.GetBoundingBoxes(coords))
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
					diff = -(Entity.BoundingBox.Min.Y - extent);
				else
					diff = extent - Entity.BoundingBox.Max.Y;

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
						if (!blockState.Block.Solid)
							continue;

						var coords = new Vector3(x, y, z);
						
						foreach (var box in blockState.Block.GetBoundingBoxes(coords))
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
					diff = -(Entity.BoundingBox.Min.X - collisionExtent.Value);
				else
					diff = (collisionExtent.Value - Entity.BoundingBox.Max.X);

				result = (float) diff;

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
						if (!blockState.Block.Solid)
							continue;

						var coords = new Vector3(x, y, z);
						
						foreach (var box in blockState.Block.GetBoundingBoxes(coords))
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

	public class ColoredBoundingBox
	{
		public BoundingBox Box   { get; }
		public Color       Color { get; }
		public ColoredBoundingBox(BoundingBox box, Color color)
		{
			Box = box;
			Color = color;
		}
	}
}