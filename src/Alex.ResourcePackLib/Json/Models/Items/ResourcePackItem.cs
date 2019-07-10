using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Models.Items
{
    public class ResourcePackItem
    {
	    public ResourcePackItem()
	    {

	    }

		[JsonProperty("parent")]
	    public string ParentName;

	    [JsonIgnore] public ResourcePackItem Parent = null;

	    public Dictionary<string, string> Textures = new Dictionary<string, string>();
		public Dictionary<string, DisplayElement> Display = new Dictionary<string, DisplayElement>();
	    public Override[] Overrides = new Override[0];
	    public BlockModelElement[] Elements { get; set; } = new BlockModelElement[0];

		[JsonIgnore]
	    public string Name { get; set; }

		[JsonIgnore]
	    public string Namespace { get; set; }
    }
}
