using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
			if (Math.Abs(entity.Velocity.X) < 0.1 * dt)
				entity.Velocity = new Vector3(0, entity.Velocity.Y, entity.Velocity.Z);
			
			if (Math.Abs(entity.Velocity.Y) < 0.1f * dt)
				entity.Velocity = new Vector3(entity.Velocity.X, 0, entity.Velocity.Z);
			
			if (Math.Abs(entity.Velocity.Z) < 0.1 * dt)
				entity.Velocity = new Vector3(entity.Velocity.X, entity.Velocity.Y, 0);
			
			//entity.Velocity.Clamp();
		}

		Stopwatch sw = new Stopwatch();
		public void Update(GameTime elapsed)
		{
			float dt = ((float) elapsed.ElapsedGameTime.TotalSeconds);
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
						bool wasColliding = e.IsCollidingWithWorld;

						//TruncateVelocity(e, dt);
						
						var velocity = e.Velocity;

						if (e.IsInWater && velocity.Y < 0f)
						{
							velocity.Y *= 0.8f;
						}
						else if (e.IsInLava)
						{
						//	velocity.Y *= 0.5f;
						}

						if (!e.IsFlying && !e.KnownPosition.OnGround && e.IsAffectedByGravity)
						{
							velocity -= new Vector3(0f, (float) (e.Gravity * dt), 0f);

							//var modifier = new Vector3(1f, (float) (1f - (e.Gravity * dt)), 1f);
							//velocity *= modifier;
						}

						var rawDrag = (float) (1f - ((e.Drag * dt)));
						
						velocity *= new Vector3(rawDrag, 1f, rawDrag);
						
						var position = e.KnownPosition;

						var preview = position.PreviewMove(velocity * dt);

						if (e.IsSneaking && e.KnownPosition.OnGround)
						{
							var newFeetBlock = e.Level?.GetBlock(preview + Vector3.Down);

							if (!newFeetBlock.Solid)
							{
								velocity = new Vector3(-velocity.X, velocity.Y, -velocity.Z);
							}
						}

						bool onGround = e.KnownPosition.OnGround;
						
					//	if (velocity != Vector3.Zero)
						{
							var boundingBox = e.GetBoundingBox(preview);

							Bound bound = new Bound(World, boundingBox, preview);

							if (bound.GetIntersecting(boundingBox, out var blocks))
							{
								velocity = AdjustForY(
									e.GetBoundingBox(new Vector3(position.X, preview.Y, position.Z)), blocks, velocity,
									position, out float? yCollisionPoint);

								if (yCollisionPoint.HasValue)
								{
									if (yCollisionPoint > position.Y)
									{
										//We hit our head.
										onGround = false;
									}
									else
									{
										if (!onGround)
										{
											onGround = true;
										}
									}
								}
								
								Hit.AddRange(blocks.Select(x => x.box));

								//var solid = blocks.Where(b => b.block.Solid && b.box.Max.Y > position.Y).ToArray();
								var solid = blocks.Where(
									b => b.block.Solid && b.box.Max.Y > position.Y && b.block.CanCollide()).ToArray();

								if (solid.Length > 0)
								{
									var heighest = solid.OrderByDescending(x => x.box.Max.Y).FirstOrDefault();

									if (MathF.Abs(heighest.box.Max.Y - boundingBox.Min.Y) <= 0.65f
									    && e.KnownPosition.OnGround && !e.IsFlying)
									{
										//if (!heighest.block.BlockState.Model
										//	.GetIntersecting(heighest.coordinates, boundingBox)
										//	.Any(x => x.Max.Y > heighest.box.Max.Y))
										//if (!blocks.Any(x => x.))
										{

											e.KnownPosition.Y = (float) heighest.box.Max.Y;
										}
									}

									if (!wasColliding)
									{
										//var min = Vector3.Transform(boundingBox.Min,
										//	Matrix.CreateRotationY(-MathHelper.ToRadians(position.HeadYaw)));

										//var max = Vector3.Transform(boundingBox.Max,
										//	Matrix.CreateRotationY(-MathHelper.ToRadians(position.HeadYaw)));

										var min = boundingBox.Min;
										var max = boundingBox.Max;

										var minX = min.X;
										var maxX = max.X;

										var previewMinX = new Vector3(minX, preview.Y, preview.Z);

										bool checkX = false;

										if (!solid.Any(
											x =>
											{
												var contains = x.box.Contains(previewMinX);

												return contains != ContainmentType.Contains
												       && contains != ContainmentType.Intersects;
											}))
										{
											previewMinX = new Vector3(maxX, preview.Y, preview.Z);

											if (solid.Any(
												x =>
												{
													var contains = x.box.Contains(previewMinX);

													return contains != ContainmentType.Contains
													       && contains != ContainmentType.Intersects;
												}))
											{
												checkX = true;
											}
										}
										else
										{
											checkX = true;
										}

										if (checkX)
										{
											for (float x = 1f; x > 0f; x -= 0.1f)
											{
												Vector3 c = (position - preview) * new Vector3(x, 1f, 1f) + position;

												if (solid.All(
													s =>
													{
														var contains = s.box.Contains(c);

														return contains != ContainmentType.Contains
														       && contains != ContainmentType.Intersects;
													}))
												{
													velocity = new Vector3(c.X - position.X, velocity.Y, velocity.Z);

													break;
												}
											}
										}

										var minZ = min.Z;
										var maxZ = max.Z;

										var previewMinZ = new Vector3(preview.X, preview.Y, minZ);

										bool checkZ = false;

										if (!solid.Any(
											x =>
											{
												var contains = x.box.Contains(previewMinZ);

												return contains != ContainmentType.Contains
												       && contains != ContainmentType.Intersects;
											}))
										{
											previewMinZ = new Vector3(preview.X, preview.Y, maxZ);

											if (solid.Any(
												x =>
												{
													var contains = x.box.Contains(previewMinZ);

													return contains != ContainmentType.Contains
													       && contains != ContainmentType.Intersects;
												}))
											{
												checkZ = true;
											}
										}
										else
										{
											checkZ = true;
										}

										if (checkZ)
										{
											for (float x = 1f; x > 0f; x -= 0.1f)
											{
												Vector3 c = (position - preview) * new Vector3(1f, 1f, x) + position;

												if (solid.All(
													s =>
													{
														var contains = s.box.Contains(c);

														return contains != ContainmentType.Contains
														       && contains != ContainmentType.Intersects;
													}))
												{
													velocity = new Vector3(velocity.X, velocity.Y, c.Z - position.Z);

													break;
												}
											}
										}
									}
								}
							}
						}

						e.Velocity = velocity;

						var beforeMove = e.KnownPosition.ToVector3();
						e.KnownPosition.Move(velocity * dt);

					//	if (e is PlayerMob p)
						{
							e.DistanceMoved += MathF.Abs(Vector3.Distance(beforeMove, e.KnownPosition.ToVector3()));
						}
						//var rawDrag = (float) (1f - (e.Drag * dt));

						//e.Velocity = velocity;// * new Vector3(1f, 0.98f, 1f);

						//e.KnownPosition.Move(e.Velocity * dt);
						
						TruncateVelocity(e, dt);

						/*var feetBlock = e.Level.GetBlockState(e.KnownPosition.GetCoordinates3D());

						if (!feetBlock.Block.Solid)
						{
							e.KnownPosition.OnGround = false;
						}
						else
						{
							
						}*/
						
						//if (MathF.Abs(e.Velocity.Y) < 0.000001f)
						{
							e.KnownPosition.OnGround = MathF.Abs(e.Velocity.Y) < 0.000001f || onGround;
						}

						{
							//e.KnownPosition.OnGround = true;
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

		private Vector3 AdjustForY(BoundingBox box, (BlockCoordinates coordinates, Block block, BoundingBox box, bool isBlockPart)[] blocks, Vector3 velocity, PlayerLocation position, out float? pointOfCollision)
		{
			pointOfCollision = null;
			
			if (velocity.Y == 0f)
				return velocity;
			
			float? collisionPoint = null;
			bool negative = velocity.Y < 0f;
			foreach (var corner in box.GetCorners().OrderBy(x => x.Y))
			{
				foreach (var block in blocks)
				{
					var blockBox = block.box;
					
					bool pass = block.block.Solid && blockBox.Contains(corner) == ContainmentType.Contains;

					if (pass)
					{
						var heading = corner - position;
						var distance = heading.LengthSquared();
						var direction = heading / distance;

						if (negative)
						{
							if (collisionPoint == null || blockBox.Max.Y > collisionPoint.Value)
							{
								collisionPoint = block.box.Max.Y;
							}
						}
						else
						{
							if (collisionPoint == null || blockBox.Min.Y < collisionPoint.Value)
							{
								collisionPoint = block.box.Min.Y;
							}
						}
					}
				}
			}

			pointOfCollision = collisionPoint;
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
