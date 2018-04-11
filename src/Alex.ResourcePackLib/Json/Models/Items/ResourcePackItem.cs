using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Models.Items
{
    public class ResourcePackItem
    {
	    public ResourcePackItem()
	    {

	    }

	    public string Parent;
	    public Dictionary<string, string> Textures = new Dictionary<string, string>();
		public Dictionary<string, DisplayElement> Display = new Dictionary<string, DisplayElement>();
	    public Override[] Overrides = new Override[0];

		[JsonIgnore]
	    public string Name { get; set; }

		[JsonIgnore]
	    public string Namespace { get; set; }
    }
}
