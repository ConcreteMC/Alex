using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Alex.API;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Entities;
using Alex.Entities.BlockEntities;
using Alex.Graphics.Models;
using Alex.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;
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
		private World           World            { get; }
		private NetworkProvider Network          { get; }

		private Entity[] _rendered;

		public EntityManager(GraphicsDevice device, World world, NetworkProvider networkProvider)
		{
			Network = networkProvider;
			World = world;
			Device = device;
			Entities = new ConcurrentDictionary<long, Entity>();
			EntityByUUID = new ConcurrentDictionary<MiNET.Utils.UUID, Entity>();
			BlockEntities = new ConcurrentDictionary<BlockCoordinates, BlockEntity>();
			_rendered = new Entity[0];
		}

		private Stopwatch _sw = new Stopwatch();
		public void OnTick()
		{
			List<Entity> rendered = new List<Entity>(_rendered.Length);

			var entities      = Entities.Values.ToArray();
			var blockEntities = BlockEntities.Values.ToArray();

			var cameraChunkPosition = new ChunkCoordinates(World.Camera.Position);
			
			foreach (var entity in entities.Concat(blockEntities))
			{
				_sw.Restart();
				rendered.Add(entity);
				
				entity.OnTick();

				//if (!entity.IsSpawned)
				//	continue;
				/*if (Math.Abs(new ChunkCoordinates(entity.KnownPosition).DistanceTo(cameraChunkPosition))
				    > World.ChunkManager.RenderDistance)
				{
					entity.IsRendered = false;

					continue;
				}*/

				var entityBox = entity.GetVisibilityBoundingBox(entity.KnownPosition);

				if (World.Camera.BoundingFrustum.Contains(
					new Microsoft.Xna.Framework.BoundingBox(entityBox.Min, entityBox.Max)) != ContainmentType.Disjoint)
				{
					entity.IsRendered = true;
				}
				else
				{
					entity.IsRendered = false;
				}
			}

			_rendered = rendered.ToArray();
		}

		private Stopwatch _updateWatch = new Stopwatch();
		public void Update(IUpdateArgs args)
		{
			//var entities      = Entities.Values.ToArray();
			//var blockEntities = BlockEntities.Values.ToArray();

			foreach (var entity in _rendered)
			{
				_updateWatch.Restart();
				//if (entity.IsRendered)
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
			if (_rendered != null)
			{
				var blendState = args.GraphicsDevice.BlendState;

				args.GraphicsDevice.BlendState = BlendState.AlphaBlend;
				
				int renderCount = 0;
				//var entities    = _rendered.ToArray();

				foreach (var entity in _rendered)
				{
					// entity.IsRendered = true;
					if (entity.IsRendered)
					{
						entity.Render(args);

						renderCount++;
					}
				}

				EntitiesRendered = renderCount;
				args.GraphicsDevice.BlendState = blendState;
			}
		}

		private static RasterizerState RasterizerState = new RasterizerState()
		{
			//DepthBias = -0.0015f,
			CullMode = CullMode.None, 
			FillMode = FillMode.Solid, 
			DepthClipEnable = false, 
			ScissorTestEnable = true,
			MultiSampleAntiAlias = true
		};

		public void Render2D(IRenderArgs args)
		{
			if (_rendered != null)
			{
				var entities = _rendered;

				if (entities.Length == 0)
					return;
				
				
				args.SpriteBatch.Begin(
					SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.PointWrap,
					DepthStencilState.Default, RasterizerState);

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

		private void RenderNametag(IRenderArgs args, Entity entity)
		{
			string clean = entity.NameTag;

			if (string.IsNullOrWhiteSpace(clean))
				return;
			
			//var halfWidth = (float)(Width * _scale);
			
			var maxDistance = (World.ChunkManager.RenderDistance * 16f);
			
			//pos.Y = 0;
			
			var distance = MathF.Abs(Vector3.Distance(entity.KnownPosition, args.Camera.Position));
			if (distance >= (maxDistance))
			{
				return;
			}

			distance *= (distance / (maxDistance / 2f));
			var scale = MathF.Max(MathF.Min(1f - (distance / (maxDistance)), 1f), 0f);
			//distance -= ((maxDistance) / distance);

			try
			{
				Vector3 posOffset = new Vector3(0, 0f, 0);

				if (!entity.IsInvisible)
				{
					posOffset.Y += (float) (entity.Height * entity.Scale);
				}

				var cameraPosition = new Vector3(args.Camera.Position.X, 0, args.Camera.Position.Z);

				var rotation = new Vector3(entity.RenderLocation.X, 0, entity.RenderLocation.Z) - cameraPosition;
				rotation.Normalize();

				var halfWidth = (float) (entity.Width * entity.Scale);
				var pos       = entity.RenderLocation + posOffset + (-(rotation * (float)entity.Width));

				Vector2 textPosition;
				
				var screenSpace = args.GraphicsDevice.Viewport.Project(
					pos, args.Camera.ProjectionMatrix, args.Camera.ViewMatrix, Matrix.Identity);

				textPosition.X = screenSpace.X;
				textPosition.Y = screenSpace.Y;

				Vector2 renderPosition = textPosition;
				
				foreach (var str in clean.Split('\n').Reverse())
				{
					var line = str.Trim();

					if (line.Length == 0 || string.IsNullOrWhiteSpace(line))
						continue;

					var stringSize = Alex.Font.MeasureString(line, (float) scale);
					var c          = new Point((int) stringSize.X, (int) stringSize.Y);

					renderPosition.X = (int) (textPosition.X - (c.X / 2d));
					renderPosition.Y -= (c.Y);
					//renderPosition.Y = (int) ((textPosition.Y + yOffset));

					args.SpriteBatch.FillRectangle(
						new Rectangle(renderPosition.ToPoint(), c), new Color(Color.Black, 128), screenSpace.Z + 0.0000001f);

					Alex.Font.DrawString(
						args.SpriteBatch, line, renderPosition, TextColor.White, FontStyle.None,
						layerDepth: screenSpace.Z, scale: new Vector2((float) scale));
				}
			}
			finally
			{
				
			}
		}

		public void Dispose()
		{
			var entities = Entities.ToArray();
			Entities.Clear();
			EntityByUUID.Clear();

			foreach (var entity in entities)
			{
				entity.Deconstruct(out _, out _);
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
					Entities.TryRemove(e.EntityId, out e);
				}
			}
		}

		public bool AddEntity(Entity entity)
		{
			entity.Level = World;

			if (EntityByUUID.TryAdd(entity.UUID, entity))
			{
				entity.IsAlwaysShowName = false;
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
			BlockEntities.TryRemove(coordinates, out _);
		}

		public bool Remove(long id)
		{
			if (Entities.TryRemove(id, out Entity entity))
			{
				Remove(entity.UUID, false);
				entity.Dispose();

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
		    foreach(var entity in Entities.ToArray())
		    {
			    Remove(entity.Key);
		    }

		    foreach (var blockEntity in BlockEntities.ToArray())
		    {
			    BlockEntities.TryRemove(blockEntity.Key, out _);
		    }
	    }
	}
}
