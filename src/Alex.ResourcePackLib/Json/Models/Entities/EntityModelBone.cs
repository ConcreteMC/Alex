using Microsoft.Xna.Framework;

namespace Alex.ResourcePackLib.Json.Models.Entities
{
	using J = Newtonsoft.Json.JsonPropertyAttribute;
	using R = Newtonsoft.Json.Required;
	using N = Newtonsoft.Json.NullValueHandling;

    public sealed class EntityModelBone
	{
		public string Name { get; set; }
		public string Material { get; set; } = string.Empty;

		[J("parent", NullValueHandling = N.Ignore)]
		public string Parent { get; set; } = string.Empty;
		public EntityModelCube[] Cubes { get; set; }
        public Vector3 Pivot { get; set; }

		[J("locators", NullValueHandling = N.Ignore)]
        public EntityModelLocators Locators { get; set; }

        public double Inflate { get; set; } = 0.0;
        public Vector3 Rotation { get; set; } = Vector3.Zero;

		public bool NeverRender { get; set; } = false;
		public bool Mirror { get; set; } = false;
		public bool Reset { get; set; } = false;
    }

	public sealed class EntityModelLocators
	{
		[J("lead", NullValueHandling = N.Ignore)]
		public Vector3 Lead { get; set; }
    }
}
