using Microsoft.Xna.Framework;

namespace Alex.ResourcePackLib.Json.Models.Entities
{
	using J = Newtonsoft.Json.JsonPropertyAttribute;
	using R = Newtonsoft.Json.Required;
	using N = Newtonsoft.Json.NullValueHandling;

    public sealed class EntityModelCube
    {
	    [J("origin")]
        public Vector3 Origin { get; set; }
        
        [J("pivot", NullValueHandling = N.Ignore)]
        public Vector3 Pivot { get; set; }
        
        [J("rotation", NullValueHandling = N.Ignore)]
        public Vector3 Rotation { get; set; }

	    [J("size")]
        public Vector3 Size { get; set; }

	    [J("uv")]
        public Vector2 Uv { get; set; }

	    [J("mirror", NullValueHandling = N.Ignore)]
	    public bool? Mirror { get; set; }

	    [J("inflate", NullValueHandling = N.Ignore)]
	    public double Inflate { get; set; } = 1;
    }
}
