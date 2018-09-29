using Microsoft.Xna.Framework;

namespace Alex.ResourcePackLib.Json.Models.Entities
{
    public sealed class EntityModelCube
    {
		public Vector3 Origin { get; set; }
		public Vector3 Size { get; set; }
		public Vector2 Uv { get; set; }
	    public double Inflate { get; set; } = 0.0;
	    public bool Mirror { get; set; } = false;

    }
}
