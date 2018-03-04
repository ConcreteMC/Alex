using Microsoft.Xna.Framework.Graphics;

namespace Alex.ResourcePackLib.Json.Models
{
    public sealed class EntityModelCube
    {
		public JVector3 Origin { get; set; }
		public JVector3 Size { get; set; }
		public JVector2 Uv { get; set; }
	    public double Inflate = 0.0;
    }
}
