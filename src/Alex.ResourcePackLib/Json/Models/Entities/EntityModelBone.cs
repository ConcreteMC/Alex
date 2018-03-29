using Microsoft.Xna.Framework;

namespace Alex.ResourcePackLib.Json.Models.Entities
{
    public sealed class EntityModelBone
	{
		public string Name { get; set; }
		public string Material { get; set; } = string.Empty;

		public Vector3 Pivot { get; set; }
		public Vector3 Rotation { get; set; } = new Vector3(0f,0f,0f);
		public EntityModelCube[] Cubes { get; set; }

		public bool NeverRender { get; set; } = false;
		public bool Mirror { get; set; } = false;
		public bool Reset { get; set; } = false;

		public double Inflate { get; set; } = 0.0;
	}
}
