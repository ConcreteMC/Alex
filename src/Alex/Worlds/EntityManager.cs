using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Alex.Common;
using Alex.Common.Graphics;
using Alex.Common.Graphics.Typography;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Common.World;
using Alex.Entities;
using Alex.Entities.BlockEntities;
using Alex.Entities.Components;
using Alex.Entities.Events;
using Alex.Graphics.Camera;
using Alex.Graphics.Models;
using Alex.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using RocketUI;
using ContainmentType = Microsoft.Xna.Framework.ContainmentType;
using UUID = MiNET.Utils.UUID;

namespace Alex.Worlds
{
	public class EntityManager : IDisposable, ITicked
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(EntityManager));
		public static EventHandler<EntityUpdateEventArgs> EntityUpdated;
		public static EventHandler<EntityUpdateEventArgs> EntityTicked;
		
		private ConcurrentDictionary<long, Entity> Entities { get; }
		private ConcurrentDictionary<MiNET.Utils.UUID, Entity> EntityByUUID { get; }
		private ConcurrentDictionary<BlockCoordinates, BlockEntity> BlockEntities { get; }
		private GraphicsDevice Device { get; }

		public int EntityCount => Entities.Count + BlockEntities.Count;
		public int EntitiesRendered { get; private set; } = 0;

		/// <summary>
		///		The amount of calls made to DrawPrimitives in the last render call
		/// </summary>
		public int DrawCount { get; private set; } = 0;

		private World World { get; }
		private NetworkProvider Network { get; }

		private Entity[] _rendered;

		public EventHandler<Entity> EntityAdded;
		public EventHandler<Entity> EntityRemoved;

		private IOptionsProvider OptionsProvider { get; }

		public EntityManager(IServiceProvider serviceProvider,
			GraphicsDevice device,
			World world,
			NetworkProvider networkProvider)
		{
			Network = networkProvider;
			World = world;
			Device = device;
			Entities = new ConcurrentDictionary<long, Entity>();
			EntityByUUID = new ConcurrentDictionary<MiNET.Utils.UUID, Entity>();
			BlockEntities = new ConcurrentDictionary<BlockCoordinates, BlockEntity>();
			_rendered = new Entity[0];

			OptionsProvider = serviceProvider.GetService<IOptionsProvider>(); //.AlexOptions.VideoOptions.

			_spriteEffect = new BasicEffect(device)
			{
				VertexColorEnabled = true,
				TextureEnabled = true,
				LightingEnabled = false,
				FogEnabled = false,
				PreferPerPixelLighting = false
			};

			world.ChunkManager.OnChunkAdded += OnChunkAdded;
			world.ChunkManager.OnChunkRemoved += OnChunkRemoved;
		}

		private void OnChunkRemoved(object? sender, ChunkRemovedEventArgs e)
		{
			var chunk = e.Chunk;

			foreach (var blockEntity in chunk.BlockEntities)
			{
				var pos = blockEntity.Key;
				RemoveBlockEntity(new BlockCoordinates(pos.X, pos.Y, pos.Z));
			}
		}

		private void OnChunkAdded(object? sender, ChunkAddedEventArgs e)
		{
			var chunk = e.Chunk;

			foreach (var blockEntity in chunk.BlockEntities)
			{
				var pos = blockEntity.Key;
				BlockEntityFactory.ReadFrom(blockEntity.Value, World, pos);
			}
		}

		private Stopwatch _sw = new Stopwatch();

		private object _tickLock = new object();

		public static ThreadLocal<ExecutionTimer> TickTimer { get; set; } = new ThreadLocal<ExecutionTimer>(() => new ExecutionTimer("entity.tick"));
		
		public void OnTick()
		{
			if (!Monitor.TryEnter(_tickLock, 0))
				return;

			_sw.Restart();

			int ticked = 0;
			int entityCount = 0;

			try
			{
				if (World?.Camera == null)
					return;

				List<Entity> rendered = new List<Entity>(_rendered.Length);

				var entities = Entities.Values.ToArray();
				var blockEntities = BlockEntities.Values.ToArray();

				var cameraChunkPosition = World.Camera.Position;

				foreach (var entity in entities.Concat(blockEntities))
				{
					var entityPos = entity.RenderLocation;

					//if (World.Camera.BoundingFrustum.Contains(entity.GetVisibilityBoundingBox(entityPos)) != ContainmentType.Disjoint)
					if (Math.Abs(Vector3.Distance(cameraChunkPosition, entityPos)) <= Math.Min(
						    World.ChunkManager.RenderDistance,
						    OptionsProvider.AlexOptions.VideoOptions.EntityRenderDistance.Value) * 16f)
					{
						entityCount++;
						rendered.Add(entity);
					}
					else
					{
						entity.IsRendered = false;
					}
				}

				foreach (var entity in rendered)
				{
					entity.IsRendered = true;

					TimingsReport report;
					using (var tickTimer = TickTimer.Value = new ExecutionTimer("entity.tick"))
					{
						entity.OnTick();
						
						tickTimer.Stop();
						report = tickTimer.GenerateReport();
					}

					EntityTicked?.Invoke(this, new EntityUpdateEventArgs(entity, report));

					ticked++;
				}

				_rendered = rendered.ToArray();
			}
			finally
			{
				Monitor.Exit(_tickLock);

				if (_sw.Elapsed.TotalMilliseconds >= 50)
				{
					Log.Warn(
						$"Tick took {_sw.ElapsedMilliseconds}ms for {entityCount} entities of which {ticked} were ticked!");
				}
			}
		}

		private Stopwatch _updateWatch = new Stopwatch();

		public static ThreadLocal<ExecutionTimer> UpdateTimer { get; set; } = new ThreadLocal<ExecutionTimer>(() => new ExecutionTimer("entity.update"));
		public void Update(IUpdateArgs args)
		{
			var delta = Alex.DeltaTime;

			IReadOnlyCollection<Entity> toUpdate;
			toUpdate = _rendered.OrderByDescending(x => DateTime.UtcNow - x.LastUpdate).ToArray();
			//toUpdate = _rendered;

			float maxTime = delta / toUpdate.Count;
			long elapsedTime = 0;

			foreach (var entity in toUpdate)
			{
				if (elapsedTime >= delta)
					break;

				_updateWatch.Restart();
				//if (entity.IsRendered)

				//pos.Y = 0;

				if (!entity.IsRendered)
					continue;

				TimingsReport report;
				using (var timer = UpdateTimer.Value = new ExecutionTimer("entity.update"))
				{
					entity.Update(args);

					timer.Stop();
					report = timer.GenerateReport();
				}

				entity.LastUpdate = DateTime.UtcNow;

				var elapsed = _updateWatch.ElapsedMilliseconds;
				elapsedTime += elapsed;

				if (elapsed > maxTime)
				{
					Log.Warn($"Entity update took {elapsed - maxTime}ms too long. Entity={entity}");
				}

				EntityUpdated?.Invoke(this, new EntityUpdateEventArgs(entity, report));
			}
		}

		public void Render(IRenderArgs args)
		{
			//_spriteEffect.SetDistance(0.015f, args.Camera.FarDistance);
			if (_rendered != null)
			{
				using (GraphicsContext.CreateContext(
					       args.GraphicsDevice, BlendState.AlphaBlend, DepthStencilState.Default,
					       RasterizerState.CullNone))
				{
					int renderCount = 0;
					int drawCount = 0;
					//var entities    = _rendered.ToArray();

					foreach (var entity in _rendered)
					{
						// entity.IsRendered = true;
						if (entity.IsRendered && !entity.IsInvisible && entity.Scale > 0f)
						{
							drawCount += entity.Render(
								args, OptionsProvider.AlexOptions.VideoOptions.EntityCulling.Value);

							renderCount++;
						}
					}

					DrawCount = drawCount;
					EntitiesRendered = renderCount;
				}
			}
		}

		private static RasterizerState RasterizerState = new RasterizerState()
		{
			//DepthBias = -0.0015f,
			CullMode = CullMode.CullCounterClockwiseFace,
			FillMode = FillMode.Solid,
			DepthClipEnable = true,
			ScissorTestEnable = false,
			MultiSampleAntiAlias = true,
		};

		private BasicEffect _spriteEffect;

		public void Render2D(IRenderArgs args)
		{
			if (_rendered != null)
			{
				var entities = _rendered;

				if (entities.Length == 0)
					return;

				_spriteEffect.Projection = args.Camera.ProjectionMatrix;
				_spriteEffect.View = args.Camera.ViewMatrix;

				try
				{
					args.SpriteBatch.Begin(
						SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.PointWrap,
						DepthStencilState.DepthRead, RasterizerState);

					foreach (var entity in entities)
					{
						if (!entity.HideNameTag)
							RenderNametag(args, entity);
					}
				}
				finally
				{
					args.SpriteBatch.End();
				}

				foreach (var sign in entities.Where(x => x is BlockEntity))
				{
					if (sign is BlockEntity be)
					{
						be.Render2D(args);
					}
				}
			}
		}

		private static Color _backgroundColor = new Color(Color.Black, 128);

		private void RenderNametag(IRenderArgs args, Entity entity)
		{
			var lines = entity.NameTagLines;

			if (lines == null || lines.Length == 0)
				return;

			Vector3 posOffset = new Vector3(0, 0.5f, 0);

			//if (!entity.IsInvisible)
			{
				posOffset.Y += (float)((entity.Height) * entity.Scale);
			}

			var renderlocation = entity.RenderLocation.ToVector3();

			var rotation = new Vector3(renderlocation.X, renderlocation.Y + posOffset.Y, renderlocation.Z)
			               - args.Camera.Position;

			rotation.Normalize();

			var halfWidth = (float)(entity.Width * entity.Scale);
			var pos = renderlocation + posOffset + (-(rotation * halfWidth));

			//Matrix rotationMatrix = MatrixHelper.CreateRotation(args.Camera.Rotation);
			//Vector3 lookAtOffset = Vector3.Forward.Transform(rotationMatrix);

			/*
			var world = Matrix.Identity;
			world *= Matrix.CreateScale(0.25f / 16f, 0.25f / 16f, 1f);
			world *= Matrix.CreateBillboard(pos, args.Camera.Position, Vector3.Up, args.Camera.Direction);
			world.Right = -world.Right;
			world.Up = -world.Up;
			_spriteEffect.World = world;*/


			//
			//Matrix.CreateBillboard(
			//	pos, args.Camera.Position, Vector3.Up, args.Camera.Direction)
			var screenSpace = args.SpriteBatch.GraphicsDevice.Viewport.Project(
				pos, args.Camera.ProjectionMatrix, args.Camera.ViewMatrix, Matrix.Identity);

			//bool isOnScreen = args.SpriteBatch.GraphicsDevice.Viewport.Bounds.Contains((int) screenSpace.X, (int) screenSpace.Y);

			//if (!isOnScreen) return;

			//count++;
			float depth = screenSpace.Z;
			var scale = 1f / depth;

			try
			{
				int yOffset = 0;

				Vector2 renderPosition = Vector2.Zero;

				foreach (var str in lines)
				{
					var line = str;

					if (line.Length == 0)
						continue;

					var stringSize = Alex.Font.MeasureString(line, scale).ToPoint();

					renderPosition.X = screenSpace.X;
					renderPosition.Y = screenSpace.Y;
					renderPosition.X += -(stringSize.X / 2f);
					renderPosition.Y += yOffset;

					args.SpriteBatch.FillRectangle(
						new Rectangle(renderPosition.ToPoint(), stringSize), _backgroundColor * 1f, depth + 0.0000001f);

					args.SpriteBatch.DrawString(
						Alex.Font, line, renderPosition, TextColor.White, FontStyle.None, 0f, Vector2.Zero,
						Vector2.One * scale, layerDepth: depth);

					yOffset += (stringSize.Y);
				}
			}
			finally
			{
				//	args.SpriteBatch.End();
			}
		}

		private void Clear()
		{
			var blockEntities = BlockEntities.ToArray();
			BlockEntities.Clear();

			foreach (var blockEntity in blockEntities)
			{
				BlockEntities.TryRemove(blockEntity.Key, out _);
				blockEntity.Value?.Dispose();
			}

			var entities = Entities.ToArray();
			Entities.Clear();
			EntityByUUID.Clear();

			foreach (var entity in entities)
			{
				entity.Deconstruct(out _, out var e);
				e?.Dispose();
			}

			var rendered = _rendered;
			_rendered = new Entity[0];

			foreach (var entity in rendered)
			{
				entity?.Dispose();
			}
		}

		public void UnloadEntities(ChunkCoordinates coordinates)
		{
			foreach (var entity in Entities.ToArray())
			{
				if (new ChunkCoordinates(entity.Value.KnownPosition).Equals(coordinates))
				{
					Remove(entity.Value.UUID);
				}
			}
		}

		private void Remove(MiNET.Utils.UUID entity, bool removeId = true)
		{
			if (EntityByUUID.TryRemove(entity, out Entity e))
			{
				if (removeId)
				{
					if (Entities.TryRemove(e.EntityId, out e)) { }
				}

				EntityRemoved?.Invoke(this, e);
				e?.Dispose();
			}
		}

		public bool AddEntity(Entity entity)
		{
			entity.Level = World;

			if (EntityByUUID.TryAdd(entity.UUID, entity))
			{
				//entity.IsAlwaysShowName = false;
				// entity.NameTag = $"Entity_{id}";
				//entity.HideNameTag = false;

				if (entity.EntityId != -1 && !Entities.TryAdd(entity.EntityId, entity))
				{
					EntityByUUID.TryRemove(entity.UUID, out Entity _);

					return false;
				}
				else if (entity.EntityId == -1)
				{
					Log.Warn(
						$"Tried adding entity with invalid entity id: {entity.NameTag} | {entity.UUID.ToString()}");
				}

				EntityAdded?.Invoke(this, entity);

				return true;
			}

			return false;
		}

		public bool AddBlockEntity(BlockCoordinates coordinates, BlockEntity entity)
		{
			//entity.KnownPosition = coordinates;
			//entity.Block = World.GetBlockState(coordinates).Block;
			if (!BlockEntities.TryAdd(coordinates, entity))
			{
				return false;
			}

			return true;
		}

		public bool TryGetBlockEntity(BlockCoordinates coordinates, out BlockEntity entity)
		{
			return BlockEntities.TryGetValue(coordinates, out entity);
		}

		public void RemoveBlockEntity(BlockCoordinates coordinates)
		{
			if (BlockEntities.TryRemove(coordinates, out var entity))
			{
				entity?.Dispose();
			}
		}

		public bool Remove(long id)
		{
			if (Entities.TryRemove(id, out Entity entity))
			{
				Remove(entity.UUID, false);
				entity?.Dispose();

				return true;
			}

			return false;
		}

		public bool TryGet(long id, out Entity entity)
		{
			return Entities.TryGetValue(id, out entity);
		}

		public bool TryGet(UUID uuid, out Entity entity)
		{
			return EntityByUUID.TryGetValue(uuid, out entity);
		}

		public IEnumerable<Entity> GetEntities(Vector3 camPos, double radius)
		{
			return Entities.Values.ToArray().Where(
					x => x.IsRendered && Math.Abs(Vector3.DistanceSquared(x.KnownPosition.ToVector3(), camPos))
						< radius)
			   .ToArray();
		}

		public void ClearEntities()
		{
			Clear();
		}

		public void Dispose()
		{
			Log.Info($"Entitymanager disposing...");

			Clear();
		}
	}
}