using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Alex.ResourcePackLib.Json.Models.Items
{
    public class ResourcePackItem : ResourcePackModelBase
    {
	    public ResourcePackItem()
	    {

	    }

	    public Override[] Overrides = new Override[0];
    }
}
