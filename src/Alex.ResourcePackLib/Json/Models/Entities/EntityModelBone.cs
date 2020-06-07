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

		[J("locators", NullValueHandling = N.Ignore)]
        public EntityModelLocators Locators { get; set; }

        public double Inflate { get; set; } = 0.0;
        
        [J("rotation", NullValueHandling = N.Ignore)]
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        
        [J("pivot", NullValueHandling = N.Ignore)]
        public Vector3 Pivot { get; set; }
		
        [J("bind_pose_rotation", NullValueHandling = N.Ignore)]
        public Vector3 BindPoseRotation { get; set; } = Vector3.Zero;
        
		public bool NeverRender { get; set; } = false;
		public bool Mirror { get; set; } = false;
		public bool Reset { get; set; } = false;
		
		public EntityModelCube[] Cubes { get; set; }
    }

	public sealed class EntityModelLocators
	{
		[J("lead", NullValueHandling = N.Ignore)]
		public Vector3 Lead { get; set; }
    }
}
