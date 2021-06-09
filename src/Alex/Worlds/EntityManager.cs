using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Alex.Common;
using Alex.Common.Graphics;
using Alex.Common.Graphics.Typography;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Common.World;
using Alex.Entities;
using Alex.Entities.BlockEntities;
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
		
		private                 ConcurrentDictionary<long, Entity>                  Entities      { get; }
		private                 ConcurrentDictionary<MiNET.Utils.UUID, Entity>      EntityByUUID  { get; }
		private                 ConcurrentDictionary<BlockCoordinates, BlockEntity> BlockEntities { get; }
		private                 GraphicsDevice                                      Device        { get; }

		public  int             EntityCount      => Entities.Count + BlockEntities.Count;
		public  int             EntitiesRendered { get; private set; } = 0;
		
		/// <summary>
		///		The amount of calls made to DrawPrimitives in the last render call
		/// </summary>
		public int DrawCount { get; private set; } = 0;
		private World           World            { get; }
		private NetworkProvider Network          { get; }

		private Entity[] _rendered;
		
		private IOptionsProvider OptionsProvider { get; }
		public EntityManager(IServiceProvider serviceProvider, GraphicsDevice device, World world, NetworkProvider networkProvider)
		{
			Network = networkProvider;
			World = world;
			Device = device;
			Entities = new ConcurrentDictionary<long, Entity>();
			EntityByUUID = new ConcurrentDictionary<MiNET.Utils.UUID, Entity>();
			BlockEntities = new ConcurrentDictionary<BlockCoordinates, BlockEntity>();
			_rendered = new Entity[0];
			
			OptionsProvider = serviceProvider.GetService<IOptionsProvider>();//.AlexOptions.VideoOptions.

			_spriteEffect = new BasicEffect(device)
			{
				VertexColorEnabled = true,
				TextureEnabled = true
			};
		}

		private Stopwatch _sw = new Stopwatch();

		public void OnTick()
		{
			_sw.Restart();

			int ticked = 0;
			int entityCount = 0;

			try
			{
				List<Entity> rendered = new List<Entity>(_rendered.Length);

				var entities = Entities.Values.ToArray();
				var blockEntities = BlockEntities.Values.ToArray();

				var cameraChunkPosition = World.Camera.Position;

				foreach (var entity in entities.Concat(blockEntities))
				{
					//entity.OnTick();

					//if (!entity.IsSpawned)
					//	continue;
					var entityPos = entity.KnownPosition;

					if (Math.Abs(Vector3.Distance(cameraChunkPosition, entityPos)) <= Math.Min(
						World.ChunkManager.RenderDistance,
						OptionsProvider.AlexOptions.VideoOptions.EntityRenderDistance.Value) * 16f)
					{
						entityCount++;
						rendered.Add(entity);

						var entityBox = entity.GetVisibilityBoundingBox(entityPos);

						if (World.Camera.BoundingFrustum.Contains(entityBox) != ContainmentType.Disjoint)
						{
							entity.IsRendered = true;
							entity.OnTick();

							ticked++;

							continue;
						}
					}

					entity.IsRendered = false;
				}

				_rendered = rendered.ToArray();
			}
			finally
			{
				if (_sw.Elapsed.TotalMilliseconds >= 50)
				{
					Log.Warn(
						$"Tick took {_sw.ElapsedMilliseconds}ms for {entityCount} entities of which {ticked} were ticked!");
				}
			}
		}

		private Stopwatch _updateWatch = new Stopwatch();

		public void Update(IUpdateArgs args)
		{
			//	float sw = whMultiplier;
			//	float sh = whMultiplier;
				
			//_spriteEffect.World = Matrix.Identity * Matrix.CreateScale(sw, sh, 1f);
			//_spriteEffect.View = Matrix.CreateLookAt(Vector3.Forward * 1f, Vector3.Backward * 0f, Vector3.Up);
			//_spriteEffect.Projection = Matrix.CreatePerspectiveFieldOfView(, args.GraphicsDevice.Viewport.AspectRatio, 0.01f, args.Camera.FarDistance);
			//_spriteEffect.View = Matrix.CreateLookAt(args.Camera.Position, args.Camera.Target, Vector3.Up);
			//_spriteEffect.Projection = Matrix.CreatePerspectiveOffCenter(
			//	0f, args.GraphicsDevice.Viewport.Width, args.GraphicsDevice.Viewport.Bounds.Height, 0f,
			//	Camera.NearDistance, args.Camera.FarDistance);
			
			//_spriteEffect.Projection = args.Camera.ProjectionMatrix;
			//_spriteEffect.View = args.Camera.ViewMatrix;
			//var entities      = Entities.Values.ToArray();
			//var blockEntities = BlockEntities.Values.ToArray();
			
			foreach (var entity in _rendered)
			{
				_updateWatch.Restart();
				//if (entity.IsRendered)

				//pos.Y = 0;
			
				if (!entity.IsRendered)
					continue;
				
				entity.Update(args);
				
				var elapsed = _updateWatch.ElapsedMilliseconds;

				if (elapsed > 13)
				{
					Log.Warn($"Entity update took to long! Spent {elapsed}ms on entity of type {entity} (EntityId={entity.EntityId})");
				}
			}
		}

		public void Render(IRenderArgs args)
		{
			//_spriteEffect.SetDistance(0.015f, args.Camera.FarDistance);
			if (_rendered != null)
			{
				var blendState = args.GraphicsDevice.BlendState;

				args.GraphicsDevice.BlendState = BlendState.Opaque;
				
				int renderCount = 0;
				int drawCount = 0;
				//var entities    = _rendered.ToArray();

				foreach (var entity in _rendered)
				{
					// entity.IsRendered = true;
					if (entity.IsRendered && !entity.IsInvisible && entity.Scale > 0f)
					{
						drawCount += entity.Render(args, OptionsProvider.AlexOptions.VideoOptions.EntityCulling.Value);

						renderCount++;
					}
				}

				DrawCount = drawCount;
				EntitiesRendered = renderCount;
				args.GraphicsDevice.BlendState = blendState;
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
				
				
				args.SpriteBatch.Begin(
					SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.PointWrap,
					DepthStencilState.DepthRead, RasterizerState);

				try
				{
					foreach (var entity in entities)
					{
						if (!entity.IsRendered)
							continue;
						
						if (!entity.HideNameTag || entity.IsAlwaysShowName)
							RenderNametag(args, entity);
						
						if (entity is BlockEntity be)
							be.Render2D(args);
					}
				}
				finally
				{
					args.SpriteBatch.End();
				}
			}
		}

		private static Color _backgroundColor = new Color(Color.Black, 128);

		private void RenderNametag(IRenderArgs args, Entity entity)
		{
			//var maxDistance = (World.ChunkManager.RenderDistance * 16f);
			var lines = entity.NameTagLines;
		//	string clean = entity.NameTag;

			if (lines == null || lines.Length == 0)
				return;

			Vector3 posOffset = new Vector3(0,  0.2f, 0);

			//if (!entity.IsInvisible)
			{
				posOffset.Y += (float) ((entity.Height) * entity.Scale);
			}

			var cameraPosition = new Vector3(args.Camera.Position.X, args.Camera.Position.Y, args.Camera.Position.Z);

			var rotation = new Vector3(
				               entity.RenderLocation.X, entity.RenderLocation.Y + posOffset.Y, entity.RenderLocation.Z)
			               - cameraPosition;

			rotation.Normalize();

			var halfWidth = (float) (entity.Width * entity.Scale);
			var pos = entity.RenderLocation + posOffset + (-(rotation * (float) halfWidth));

			Vector2 textPosition;

			var screenSpace = args.GraphicsDevice.Viewport.Project(
				pos, args.Camera.ProjectionMatrix, args.Camera.ViewMatrix, Matrix.Identity);

			bool isOnScreen = args.GraphicsDevice.Viewport.Bounds.Contains((int) screenSpace.X, (int) screenSpace.Y);

			if (!isOnScreen) return;

			textPosition.X = screenSpace.X;
			textPosition.Y = screenSpace.Y;

			Vector2 renderPosition = textPosition;
			//var s = new Vector2((float) scale);

			// Compute the depth and scale of the object.
			float depth = screenSpace.Z;
			
			//Log.Info($"Depth: {depth}");
			//float scale = 1.0f / depth;
			
			float scale =  1f - (Vector3.Distance(args.Camera.Position, pos) / args.Camera.FarDistance);// 1.0f / depth;
			if (scale <= 0f)
				return;
			
			var s = new Vector2(scale, scale);
			
			foreach (var str in lines)
			{
				var line = str;

				if (line.Length == 0 || string.IsNullOrWhiteSpace(line))
					continue;

				var stringSize = Alex.Font.MeasureString(line, scale).ToPoint();
				//var c = new Point((int) stringSize.X, (int) stringSize.Y);

				renderPosition.X = textPosition.X - (stringSize.X / 2f);
				renderPosition.Y -= (stringSize.Y);
				//renderPosition.Y = (int) ((textPosition.Y + yOffset));

				args.SpriteBatch.FillRectangle(
					new Rectangle(renderPosition.ToPoint(), stringSize), _backgroundColor * 1f, depth + 0.0000001f);

				args.SpriteBatch.DrawString(
					Alex.Font, line, renderPosition, TextColor.White, FontStyle.None, 0f, Vector2.Zero, s, 1f,
					SpriteEffects.None, depth);

				//Alex.Font.DrawString(
				//	args.SpriteBatch, line, renderPosition, (Color) TextColor.White, FontStyle.None, layerDepth: depth,
				///	scale: new Vector2(scale, scale), opacity:opacity);
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
					if (Entities.TryRemove(e.EntityId, out e))
					{
						
					}
				}
				
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
				}else if (entity.EntityId == -1)
				{
					Log.Warn($"Tried adding entity with invalid entity id: {entity.NameTag} | {entity.UUID.ToString()}");
				}

				return true;
			}

			return false;
		}

		public bool AddBlockEntity(BlockCoordinates coordinates, BlockEntity entity)
		{
			entity.KnownPosition = coordinates;
			entity.Block = World.GetBlockState(coordinates).Block;
			return BlockEntities.TryAdd(coordinates, entity);
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

	    public IEnumerable<Entity> GetEntities(Vector3 camPos, int radius)
	    {
		    return Entities.Values.ToArray().Where(x => x.IsRendered && Math.Abs(Vector3.DistanceSquared(x.KnownPosition.ToVector3(), camPos)) < radius).ToArray();
	    }

	    public void ClearEntities()
	    {
		    Clear();
	    }
	    
	    public void Dispose()
	    {
		    Clear();
	    }
	}
}
