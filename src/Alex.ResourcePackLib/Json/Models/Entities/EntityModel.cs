using Microsoft.Xna.Framework;

namespace Alex.ResourcePackLib.Json.Models.Entities
{
	using J = Newtonsoft.Json.JsonPropertyAttribute;
	using R = Newtonsoft.Json.Required;
	using N = Newtonsoft.Json.NullValueHandling;

    public class EntityModel
    {
		public string Name { get; set; }

	    [J("visible_bounds_width", NullValueHandling = N.Ignore)]
	    public long VisibleBoundsWidth { get; set; }

	    [J("visible_bounds_height", NullValueHandling = N.Ignore)]
	    public long VisibleBoundsHeight { get; set; }

        [J("visible_bounds_offset", NullValueHandling = N.Ignore)]
		public Vector3 VisibleBoundsOffset { get; set; }

	    [J("texturewidth", NullValueHandling = N.Ignore)]
	    public long Texturewidth { get; set; }

	    [J("textureheight", NullValueHandling = N.Ignore)]
	    public long Textureheight { get; set; }

	    [J("bones")]
        public EntityModelBone[] Bones { get; set; }
	}
}
