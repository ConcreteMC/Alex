using Microsoft.Xna.Framework;
using Newtonsoft.Json;

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

	    [J("inflate", NullValueHandling = N.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
	    public double Inflate { get; set; } = 0;
	    
	    public Vector3 InflatedSize 
	    {
		    get
		    {
			    var inflation = (float) Inflate;
			    var size    = new Vector3(Size.X, Size.Y, Size.Z);

			    if (size.X < 0) size.X -= inflation;
			    else size.X += inflation;
			    
			    if (size.Y < 0) size.Y -= inflation;
			    else size.Y += inflation;
			    
			    if (size.Z < 0) size.Z -= inflation;
			    else size.Z += inflation;
			    
			    return size;
		    }
	    }

	    public Vector3 InflatedOrigin
	    {
		    get
		    {
			    var inflation = (float) Inflate / 2f;
			    var origin = new Vector3(Origin.X, Origin.Y, Origin.Z);

			    if (origin.X < 0) origin.X -= inflation;
			    else origin.X += inflation;
			    
			    if (origin.Y < 0) origin.Y -= inflation;
			    else origin.Y += inflation;
			    
			    if (origin.Z < 0) origin.Z -= inflation;
			    else origin.Z += inflation;
			    
			    return origin;
		    }
	    }
    }
}
