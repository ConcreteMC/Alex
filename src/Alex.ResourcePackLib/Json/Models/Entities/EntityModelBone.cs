using Microsoft.Xna.Framework;

namespace Alex.ResourcePackLib.Json.Models.Entities
{
    public sealed class EntityModelBone
	{
		public string Name { get; set; }

		public Vector3 Pivot { get; set; }
		public Vector3 Rotation { get; set; }
		public EntityModelCube[] Cubes { get; set; }

		public bool NeverRender { get; set; } = false;
	}
}
