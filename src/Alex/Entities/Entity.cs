using Alex.Graphics.Models;
using Microsoft.Xna.Framework;

namespace Alex.Entities
{
    public class Entity
    {
		public Vector3 Position { get; protected set; } = Vector3.Zero;
		public string Model { get; protected set; }
		internal EntityModelRenderer ModelRenderer { get; }
	    protected Entity()
	    {

	    }
    }
}