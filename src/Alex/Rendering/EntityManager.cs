using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Alex.API.Graphics;
using Alex.Entities;
using Alex.Gamestates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Rendering
{
    public class EntityManager
    {
		private ConcurrentDictionary<long, Entity> Entities { get; }
		private GraphicsDevice Device { get; }
	    public EntityManager(GraphicsDevice device)
	    {
		    Device = device;
			Entities = new ConcurrentDictionary<long, Entity>();
	    }

	    public void Update(GameTime gameTime)
	    {
		    var entities = Entities.Values.ToArray();
		    foreach (var entity in entities)
		    {
				entity.ModelRenderer?.Update(Device, gameTime);
		    }
	    }

	    public void Render(IRenderArgs args, Camera.Camera camera)
	    {
		    var entities = Entities.Values.ToArray();
		    foreach (var entity in entities)
		    {
				entity.ModelRenderer?.Render(args, camera, entity.Position);
		    }
	    }
    }
}
