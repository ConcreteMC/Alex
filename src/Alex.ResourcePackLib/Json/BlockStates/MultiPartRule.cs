using Alex.ResourcePackLib.Json.Converters;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.BlockStates
{
    public sealed class MultiPartRule
    {
		public string North = "";
	    public string South = "";
	    public string East = "";
	    public string West = "";
	    public string Up = "";
	    public string Down = "";

	    [JsonConverter(typeof(SingleOrArrayConverter<MultiPartRule>))]
	    public MultiPartRule[] Or = null;

	    [JsonConverter(typeof(SingleOrArrayConverter<MultiPartRule>))]
	    public MultiPartRule[] And = null;

	    public bool HasOrContition => Or != null && Or.Length >= 1;

	    public bool HasAndContition => And != null && And.Length >= 1;
	}

	
}
