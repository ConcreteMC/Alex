using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Converters;
using Microsoft.Xna.Framework;
using MiNET.Utils.Skins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Alex.ResourcePackLib.Json.Models.Entities
{
	using J = Newtonsoft.Json.JsonPropertyAttribute;
	using R = Newtonsoft.Json.Required;
	using N = Newtonsoft.Json.NullValueHandling;

    public class OldEntityModel : EntityModel
    {
	    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

	    public string Name {
		    get
		    {
			    return Description.Identifier;
		    }
		    set
		    {
			    Description.Identifier = value;
		    } 
	    }

	    [J("visible_bounds_width", NullValueHandling = N.Ignore)]
	    public double VisibleBoundsWidth {
		    get
		    {
			    return Description.VisibleBoundsWidth;
		    }
		    set
		    {
			    Description.VisibleBoundsWidth = value;
		    } 
	    }

	    [J("visible_bounds_height", NullValueHandling = N.Ignore)]
	    public double VisibleBoundsHeight {
		    get
		    {
			    return Description.VisibleBoundsHeight;
		    }
		    set
		    {
			    Description.VisibleBoundsHeight = value;
		    } 
	    }

        [J("visible_bounds_offset", NullValueHandling = N.Ignore)]
		public Vector3 VisibleBoundsOffset {
			get
			{
				return Description.VisibleBoundsOffset;
			}
			set
			{
				Description.VisibleBoundsOffset = value;
			} 
		}

	    [J("texturewidth", NullValueHandling = N.Ignore)]
	    public long Texturewidth
	    {
		    get
		    {
			    return Description.TextureWidth;
		    }
		    set
		    {
			    Description.TextureWidth = value;
		    } 
	    }

	    [J("textureheight", NullValueHandling = N.Ignore)]
	    public long Textureheight 
	    {
		    get
		    {
			    return Description.TextureHeight;
		    }
		    set
		    {
			    Description.TextureHeight = value;
		    } 
	    }

	    public OldEntityModel()
	    {
		    Description = new ModelDescription();
	    }
    }

    public class EntityModel
    {
	    [JsonProperty("description")]
	    public ModelDescription Description { get; set; }
	    
	    [J("bones")]
	    public EntityModelBone[] Bones { get; set; }
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
