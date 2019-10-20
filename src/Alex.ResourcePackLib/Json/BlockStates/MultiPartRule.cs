using Alex.ResourcePackLib.Json.Converters;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.BlockStates
{
	public sealed class MultiPartRule
    {
	    [JsonConverter(typeof(StringBooleanConverter))]
	    public string North = "";
	    
	    [JsonConverter(typeof(StringBooleanConverter))]
	    public string South = "";
	    
	    [JsonConverter(typeof(StringBooleanConverter))]
	    public string East = "";
	    
	    [JsonConverter(typeof(StringBooleanConverter))]
	    public string West = "";
	    
	    [JsonConverter(typeof(StringBooleanConverter))]
	    public string Up = "";
	    
	    [JsonConverter(typeof(StringBooleanConverter))]
	    public string Down = "";

	    [JsonConverter(typeof(SingleOrArrayConverter<MultiPartRule>))]
	    public MultiPartRule[] Or = null;

	    [JsonConverter(typeof(SingleOrArrayConverter<MultiPartRule>))]
	    public MultiPartRule[] And = null;

	    public bool HasOrContition => Or != null && Or.Length >= 1;

	    public bool HasAndContition => And != null && And.Length >= 1;
	}

	
}
