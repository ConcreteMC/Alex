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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ContainmentType = Microsoft.Xna.Framework.ContainmentType;

namespace Alex.Worlds
{
    public class EntityManager : IEntityHolder, IDisposable
	{
		private ConcurrentDictionary<long, IEntity> Entities { get; }
		private ConcurrentDictionary<UUID, IEntity> EntityByUUID { get; }
		private GraphicsDevice Device { get; }

	    public int EntityCount => Entities.Count;
	    public int EntitiesRendered { get; private set; } = 0;
		private World World { get; }
		private INetworkProvider Network { get; }

		public EntityManager(GraphicsDevice device, World world, INetworkProvider networkProvider)
		{
			Network = networkProvider;
		    World = world;
		    Device = device;
			Entities = new ConcurrentDictionary<long, IEntity>();
			EntityByUUID = new ConcurrentDictionary<UUID, IEntity>();
	    }

	    public void Update(IUpdateArgs args, SkyBox skyRenderer)
	    {
		    var entities = Entities.Values.ToArray();
		    foreach (var entity in entities)
		    {
			    if (entity is Entity e)
			    {
				    e.ModelRenderer.DiffuseColor = Color.White.ToVector3() * World.BrightnessModifier;
			    }
				entity.Update(args);
		    }
	    }

	    public void Render(IRenderArgs args)
	    {
		    int renderCount = 0;
		    var entities = Entities.Values.ToArray();
		    foreach (var entity in entities)
		    {
			    var entityBox = entity.GetBoundingBox();

				if (args.Camera.BoundingFrustum.Contains(new Microsoft.Xna.Framework.BoundingBox(entityBox.Min, entityBox.Max)) != ContainmentType.Disjoint)
			    {
				    entity.Render(args);
				    renderCount++;
			    }
		    }

		    EntitiesRendered = renderCount;
	    }

	    public void Render2D(IRenderArgs args)
	    {
		    var entities = Entities.Values.ToArray();
		    foreach (var entity in entities)
		    {
			    if (entity is PlayerMob player)
			    {
				    var entityBox = player.GetBoundingBox();

				    if (args.Camera.BoundingFrustum.Contains(
					        new Microsoft.Xna.Framework.BoundingBox(entityBox.Min, entityBox.Max)) !=
				        ContainmentType.Disjoint)
				    {
					    player.RenderNametag(args);
				    }
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
				entity.Deconstruct(out long _, out IEntity _);
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
		    if (EntityByUUID.TryRemove(entity, out IEntity e))
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
			    entity.HideNameTag = false;

			    if (!Entities.TryAdd(id, entity))
			    {
				    EntityByUUID.TryRemove(entity.UUID, out IEntity _);
				    return false;
			    }

			    return true;
		    }

		    return false;
	    }

	    public void Remove(long id)
	    {
		    if (Entities.TryRemove(id, out IEntity entity))
		    {
				Remove(entity.UUID, false);
		    }
	    }

	    public bool TryGet(long id, out IEntity entity)
	    {
		    return Entities.TryGetValue(id, out entity);
	    }


	    public IEnumerable<IEntity> GetEntities(Vector3 camPos, int radius)
	    {
		    return Entities.Values.ToArray().Where(x => Math.Abs(x.KnownPosition.DistanceTo(new PlayerLocation(camPos))) < radius).ToArray();
	    }
	}
}
