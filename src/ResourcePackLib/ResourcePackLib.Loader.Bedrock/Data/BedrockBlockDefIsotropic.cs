using Newtonsoft.Json;

namespace ResourcePackLib.Loader.Bedrock.Data;

[JsonConverter(typeof(BedrockBlockDefIsotropicJsonConverter))]
public class BedrockBlockDefIsotropic
{
    [JsonProperty("up", NullValueHandling = NullValueHandling.Ignore)]
    public bool Up { get; set; }

    [JsonProperty("down", NullValueHandling = NullValueHandling.Ignore)]
    public bool Down { get; set; }

    public BedrockBlockDefIsotropic()
    {
    }

    public BedrockBlockDefIsotropic(bool both) : this(both, both)
    {
    }

    public BedrockBlockDefIsotropic(bool up, bool down) : this()
    {
        Up = up;
        Down = down;
    }
}