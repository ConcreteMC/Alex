using Alex.ResourcePackLib.Json.Converters;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.BlockStates
{
    public class MultiPartRule
    {
	    public string North = "";
	    public string South = "";
	    public string East = "";
	    public string West = "";
	    public string Up = "";
	    public string Down = "";

	    [JsonConverter(typeof(BlockStateMultipartRuleConverter))]
		public MultiPartRule[] Or = null;
    }

	
}
