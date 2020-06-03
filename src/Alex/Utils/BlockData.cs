using System.Collections.Generic;
using Newtonsoft.Json;

namespace Alex
{
    public class BlockData
    {
	    public IDictionary<string, string[]> Properties;
	    public State[] States;

	    public class State
	    {
		    public IDictionary<string, string> Properties;
		    public uint ID;
		    public bool Default;
	    }

	    public static IDictionary<string, BlockData> FromJson(string json)
	    {
		    return JsonConvert.DeserializeObject<Dictionary<string, BlockData>>(json);
	    }
    }
}
