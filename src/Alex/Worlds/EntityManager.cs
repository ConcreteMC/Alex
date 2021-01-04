using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Alex.API.Graphics;
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
		public  long            VertexCount      { get; private set; }
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
				
				entity.OnTick();

				if (Math.Abs(new ChunkCoordinates(entity.KnownPosition).DistanceTo(cameraChunkPosition))
				    > World.ChunkManager.RenderDistance)
				{
					entity.IsRendered = false;

					continue;
				}

				var entityBox = entity.GetVisibilityBoundingBox(entity.RenderLocation);

				if (World.Camera.BoundingFrustum.Contains(
					new Microsoft.Xna.Framework.BoundingBox(entityBox.Min, entityBox.Max)) != ContainmentType.Disjoint)
				{
					rendered.Add(entity);
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

				long vertexCount = 0;
				int  renderCount = 0;


				var entities = _rendered.ToArray();

				foreach (var entity in entities)
				{
					// entity.IsRendered = true;

					entity.Render(args);
					vertexCount += entity.RenderedVertices;

					renderCount++;
				}

				EntitiesRendered = renderCount;
				VertexCount = vertexCount;

				args.GraphicsDevice.BlendState = blendState;
			}
		}

		private static RasterizerState RasterizerState = new RasterizerState()
		{
			//DepthBias = -0.0015f,
			CullMode = CullMode.None, FillMode = FillMode.Solid, DepthClipEnable = true, ScissorTestEnable = true
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
					DepthStencilState.DepthRead, RasterizerState);

				try
				{
					foreach (var entity in entities)
					{
						if (!entity.HideNameTag)
							entity.RenderNametag(args);
					}
				}
				finally
				{
					args.SpriteBatch.End();
				}
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

		public bool AddEntity(long id, Entity entity)
		{
			entity.Network = Network;
			entity.Level = World;

			if (EntityByUUID.TryAdd(entity.UUID, entity))
			{
				entity.IsAlwaysShowName = false;
				// entity.NameTag = $"Entity_{id}";
				//entity.HideNameTag = false;

				if (!Entities.TryAdd(id, entity))
				{
					EntityByUUID.TryRemove(entity.UUID, out Entity _);

					return false;
				}

				return true;
			}

			return false;
		}

		public bool AddBlockEntity(BlockCoordinates coordinates, BlockEntity entity)
		{
			entity.KnownPosition = coordinates;
			entity.Block = World.GetBlock(coordinates);
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
		    return Entities.Values.ToArray().Where(x => x.IsRendered && Math.Abs(x.KnownPosition.DistanceTo(new PlayerLocation(camPos))) < radius).ToArray();
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
