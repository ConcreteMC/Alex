using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Models.Blocks
{
    public class BedrockBlockModel
    {
	    public BedrockBlockModel()
	    {

	    }

	    [JsonProperty("blockshape")]
	    public string Blockshape { get; set; }

	    [JsonProperty("sound")]
	    public string Sound { get; set; }
	}
}
