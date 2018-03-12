using Microsoft.Xna.Framework;

namespace Alex.ResourcePackLib.Json.Models.Entities
{
    public sealed class EntityModelCube
    {
		public Vector3 Origin { get; set; }
		public Vector3 Size { get; set; }
		public Vector2 Uv { get; set; }
	    public double Inflate = 0.0;
    }
}
