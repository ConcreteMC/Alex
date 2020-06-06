using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Models.Entities
{
	using J = Newtonsoft.Json.JsonPropertyAttribute;
	using R = Newtonsoft.Json.Required;
	using N = Newtonsoft.Json.NullValueHandling;

    public class EntityModel
    {
		public string Name { get; set; }

	    [J("visible_bounds_width", NullValueHandling = N.Ignore)]
	    public double VisibleBoundsWidth { get; set; }

	    [J("visible_bounds_height", NullValueHandling = N.Ignore)]
	    public double VisibleBoundsHeight { get; set; }

        [J("visible_bounds_offset", NullValueHandling = N.Ignore)]
		public Vector3 VisibleBoundsOffset { get; set; }

	    [J("texturewidth", NullValueHandling = N.Ignore)]
	    public long Texturewidth { get; set; }

	    [J("textureheight", NullValueHandling = N.Ignore)]
	    public long Textureheight { get; set; }

	    [J("bones")]
        public EntityModelBone[] Bones { get; set; }
	}

    public class NewEntityModel : EntityModel
    {
	    [JsonProperty("description")]
	    public ModelDescription Description { get; set; }
    }
    
    public partial class ModelDescription
    {
	    [J("identifier")]
	    public string Identifier { get; set; }

	    [JsonProperty("texture_width")]
	    public long TextureWidth { get; set; }

	    [JsonProperty("texture_height")]
	    public long TextureHeight { get; set; }

	    [JsonProperty("visible_bounds_offset", NullValueHandling = N.Ignore)]
	    public Vector3 VisibleBoundsOffset { get; set; }

	    [JsonProperty("visible_bounds_width")]
	    public double VisibleBoundsWidth { get; set; }
	    
	    [J("visible_bounds_height")]
	    public double VisibleBoundsHeight { get; set; }
    }
}
