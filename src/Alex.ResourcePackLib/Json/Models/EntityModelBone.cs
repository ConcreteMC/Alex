using Microsoft.Xna.Framework.Graphics;

namespace Alex.ResourcePackLib.Json.Models
{
    public sealed class EntityModelBone
	{
		public string Name { get; set; }

		public JVector3 Pivot { get; set; }
		public JVector3 Rotation { get; set; }
		public EntityModelCube[] Cubes { get; set; }

		public bool NeverRender { get; set; } = false;
	}
}
