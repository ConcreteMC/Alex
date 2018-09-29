using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.ResourcePackLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Alex
{
    public static class ItemFactory
    {
		private static Dictionary<string, ItemMapper> _javaItemMappers = new Dictionary<string, ItemMapper>();

	    public static void Init(ResourceManager resources, McResourcePack resourcePack)
	    {
		    _javaItemMappers = JsonConvert.DeserializeObject<Dictionary<string, ItemMapper>>(Resources.Items);

	    }

	    public static bool ResolveItemName(int protocolId, out string res)
	    {
		    var result = _javaItemMappers.FirstOrDefault(x => x.Value.ProtocolId == protocolId).Key;
		    if (result != null)
		    {
			    res = result;
			    return true;
		    }

		    res = null;
		    return false;
	    }

	    private class ItemMapper
	    {
			[JsonProperty("protocol_id")]
			public int ProtocolId { get; set; }
	    }
    }
}
