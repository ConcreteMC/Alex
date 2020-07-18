using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Entities;
using Alex.API.Graphics;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Entities;
using Alex.Graphics.Models;
using Alex.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ContainmentType = Microsoft.Xna.Framework.ContainmentType;

namespace Alex.Worlds
{
    public class EntityManager : IDisposable
	{
		private ConcurrentDictionary<long, Entity> Entities { get; }
		private ConcurrentDictionary<UUID, Entity> EntityByUUID { get; }
		private GraphicsDevice Device { get; }

	    public int EntityCount => Entities.Count;
	    public int EntitiesRendered { get; private set; } = 0;
	    public long VertexCount { get; private set; }
		private World World { get; }
		private NetworkProvider Network { get; }
		
		private Entity[] _rendered;
		public EntityManager(GraphicsDevice device, World world, NetworkProvider networkProvider)
		{
			Network = networkProvider;
		    World = world;
		    Device = device;
			Entities = new ConcurrentDictionary<long, Entity>();
			EntityByUUID = new ConcurrentDictionary<UUID, Entity>();
		}

		public void Tick()
		{
			List<Entity> rendered = new List<Entity>();
			
			var entities = Entities.Values.ToArray();

			foreach (var entity in entities)
			{
				entity.OnTick();

				var entityBox = entity.GetBoundingBox();
				if (World.Camera.BoundingFrustum.Contains(new Microsoft.Xna.Framework.BoundingBox(entityBox.Min, entityBox.Max)) != ContainmentType.Disjoint)
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
		
	    public void Update(IUpdateArgs args, SkyBox skyRenderer)
	    {
		    var lightColor = new Color(245, 245, 225).ToVector3();
		    
		    var entities = Entities.Values.ToArray();
		    foreach (var entity in entities)
		    {
			    if (entity.ModelRenderer != null)
				    entity.ModelRenderer.DiffuseColor = ( new Color(245, 245, 225).ToVector3() * ((1f / 16f) * entity.SurroundingLightValue))
				                                        * World.BrightnessModifier;
			    
			    entity.Update(args);
		    }
	    }

	    public void Render(IRenderArgs args)
	    {
		    var blendState = args.GraphicsDevice.BlendState;
		    
		    args.GraphicsDevice.BlendState = BlendState.AlphaBlend;
		    
		    long vertexCount = 0;
		    int renderCount = 0;
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

	    private static RasterizerState RasterizerState = new RasterizerState()
	    {
		    //DepthBias = -0.0015f,
		    CullMode = CullMode.None,
		    FillMode = FillMode.Solid,
		    DepthClipEnable = true,
		    ScissorTestEnable = true
	    };
	    
	    public void Render2D(IRenderArgs args)
	    {
		    args.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.DepthRead, RasterizerState);
		    try
		    {
			    var entities = _rendered;
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

	    private void Remove(UUID entity, bool removeId = true)
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

	    public void Remove(long id)
	    {
		    if (Entities.TryRemove(id, out Entity entity))
		    {
				Remove(entity.UUID, false);
		    }
	    }

	    public bool TryGet(long id, out Entity entity)
	    {
		    return Entities.TryGetValue(id, out entity);
	    }


	    public IEnumerable<Entity> GetEntities(Vector3 camPos, int radius)
	    {
		    return Entities.Values.ToArray().Where(x => Math.Abs(x.KnownPosition.DistanceTo(new PlayerLocation(camPos))) < radius).ToArray();
	    }

	    public void ClearEntities()
	    {
		    foreach(var entity in Entities.ToArray())
		    {
			    Remove(entity.Key);
		    }
	    }
	}
}
