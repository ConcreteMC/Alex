using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.Entities;
using Alex.Gamestates;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ContainmentType = Microsoft.Xna.Framework.ContainmentType;

namespace Alex.Rendering
{
    public class EntityManager : IDisposable
    {
		private ConcurrentDictionary<long, Entity> Entities { get; }
		private ConcurrentDictionary<UUID, Entity> EntityByUUID { get; }
		private GraphicsDevice Device { get; }

	    public int EntityCount => Entities.Count;
	    public int EntitiesRendered { get; private set; } = 0;
		private World World { get; }
	    public EntityManager(GraphicsDevice device, World world)
	    {
		    World = world;
		    Device = device;
			Entities = new ConcurrentDictionary<long, Entity>();
			EntityByUUID = new ConcurrentDictionary<UUID, Entity>();
	    }

	    public void Update(GameTime gameTime)
	    {
		    var entities = Entities.Values.ToArray();
		    foreach (var entity in entities)
		    {
				entity.ModelRenderer?.Update(Device, gameTime, entity.KnownPosition);
		    }
	    }

	    public void Render(IRenderArgs args, Camera.Camera camera)
	    {
		    int renderCount = 0;
		    var entities = Entities.Values.ToArray();
		    foreach (var entity in entities)
		    {
			    var entityBox = entity.GetBoundingBox();

				if (camera.BoundingFrustum.Contains(new Microsoft.Xna.Framework.BoundingBox(entityBox.Min, entityBox.Max)) != ContainmentType.Disjoint)
			    {
				    entity.ModelRenderer?.Render(args, camera, entity.KnownPosition);
				    renderCount++;
			    }
		    }

		    EntitiesRendered = renderCount;
	    }

	    public void Render2D(IRenderArgs args, Camera.Camera camera)
	    {
		    var entities = Entities.Values.ToArray();
		    foreach (var entity in entities.Where(x =>
			    x.IsShowName && !string.IsNullOrWhiteSpace(x.NameTag) &&
			    (x.IsAlwaysShowName || Vector3.Distance(camera.Position, x.KnownPosition) < 16f)))
		    {
			    var entityBox = entity.GetBoundingBox();

			    if (camera.BoundingFrustum.Contains(
				        new Microsoft.Xna.Framework.BoundingBox(entityBox.Min, entityBox.Max)) !=
			        ContainmentType.Disjoint)
			    {
				    entity.RenderNametag(args, camera);
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
				entity.Deconstruct(out long _, out Entity _);
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
		    if (EntityByUUID.TryAdd(entity.UUID, entity))
		    {
			    entity.IsAlwaysShowName = false;
			    entity.NameTag = $"Entity_{id}";
			    entity.HideNameTag = false;

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
    }
}
