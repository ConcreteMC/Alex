using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Converters;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Alex.ResourcePackLib.Json.Models.Entities
{
	using J = Newtonsoft.Json.JsonPropertyAttribute;
	using R = Newtonsoft.Json.Required;
	using N = Newtonsoft.Json.NullValueHandling;

    public class EntityModel
    {
	    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
	    
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
        
        public static void GetEntries(string json, Dictionary<string, EntityModel> entries)
        {
            var serializer = new JsonSerializer()
            {
                Converters = {new Vector3Converter(), new Vector2Converter()}
            };
            
            //using (var open = file.OpenText())
            {
               // var     json = open.ReadToEnd();
                JObject obj  = JObject.Parse(json, new JsonLoadSettings());

                foreach (var e in obj)
                {
                    if (e.Key == "minecraft:geometry" && e.Value.Type == JTokenType.Array)
                    {
                        var models = e.Value.ToObject<NewEntityModel[]>(serializer);
                        if (models != null)
                        {
                            foreach (var model in models)
                            {
                                model.Name = model.Description.Identifier;
                                model.Textureheight = model.Description.TextureHeight;
                                model.Texturewidth = model.Description.TextureWidth;
                                model.VisibleBoundsHeight = model.Description.VisibleBoundsHeight;
                                model.VisibleBoundsWidth = model.Description.VisibleBoundsWidth;
                                model.VisibleBoundsOffset = model.Description.VisibleBoundsOffset;
                                
                                if (!entries.TryAdd(model.Description.Identifier, model))
                                {
                                    Log.Warn($"The name {model.Description.Identifier} was already in use!");
                                }
                            }
                            
                            continue;
                        }
                    } 
                    
                    if ( /*e.Key == "format_version" || e.Value.Type == JTokenType.Array*/
                        !e.Key.StartsWith("geometry."))
                    {
                        if (e.Value.Type == JTokenType.Array)
                        {
                            continue;
                            foreach (var type in e.Value.ToObject<EntityModel[]>(serializer))
                            {
                                entries.TryAdd(e.Key, type);
                            }
                        }
                        continue;
                    }

                    //if (e.Key == "minecraft:client_entity") continue;
                    //if (e.Key.Contains("zombie")) Console.WriteLine(e.Key);
                    var newModel = e.Value.ToObject<EntityModel>(serializer);

                    if (newModel != null)
                    {
	                    newModel.Name = e.Key;
	                    entries.TryAdd(e.Key, newModel);
                    }
                }
            }
        }
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
