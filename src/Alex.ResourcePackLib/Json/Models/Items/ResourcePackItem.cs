using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Alex.ResourcePackLib.Json.Models.Items
{
    public class ResourcePackItem
    {
	    public ResourcePackItem()
	    {

	    }

		[JsonProperty("parent")]
	    public string ParentName;

	    [JsonIgnore]
	    private ResourcePackItem _parent = null;
	    
	    [JsonIgnore]
	    public ResourcePackItem Parent
	    {
		    get => _parent;
		    set
		    {
			    _parent = value;
			    UpdateValuesFromParent();
		    }
	    }

	    private void UpdateValuesFromParent()
	    {
		    if (_parent == null) return;
		    
		    if (!GuiLight.HasValue && Parent.GuiLight.HasValue)
		    {
			    GuiLight = Parent.GuiLight;
		    }

		    if (Elements.Length == 0 && _parent.Elements.Length > 0)
		    {
			    Elements = (BlockModelElement[]) _parent.Elements.Clone();
		    }

		    foreach (var kvp in _parent.Textures)
		    {
			    if (!Textures.ContainsKey(kvp.Key))
			    {
				    Textures.Add(kvp.Key, kvp.Value);
			    }
		    }

		    foreach (var kvp in _parent.Display)
		    {
			    if (!Display.ContainsKey(kvp.Key))
			    {
				    Display.Add(kvp.Key, kvp.Value);
			    }
		    }
	    }

	    public Dictionary<string, DisplayElement> Display = new Dictionary<string, DisplayElement>();
	    
	    public Dictionary<string, string> Textures = new Dictionary<string, string>();

	    [JsonProperty("gui_light"), JsonConverter(typeof(StringEnumConverter))]
	    public GuiLight? GuiLight = Alex.ResourcePackLib.Json.Models.Items.GuiLight.Front;
	    public BlockModelElement[] Elements { get; set; } = new BlockModelElement[0];

	    public Override[] Overrides = new Override[0];

	    [JsonIgnore]
	    public string Name { get; set; }

		[JsonIgnore]
	    public string Namespace { get; set; }
    }
}
