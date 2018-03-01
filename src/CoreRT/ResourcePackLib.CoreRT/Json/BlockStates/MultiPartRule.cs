using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using ResourcePackLib.CoreRT.Json.Converters;

namespace ResourcePackLib.CoreRT.Json.BlockStates
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
