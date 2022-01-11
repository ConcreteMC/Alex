using Newtonsoft.Json;

namespace ResourcePackLib.Loader.Bedrock.Data;

[JsonConverter(typeof(BedrockBlockDefTexturesJsonConverter))]
public class BedrockBlockDefTextures
{
    [JsonProperty("up", NullValueHandling = NullValueHandling.Ignore)]
    public string Up { get; set; }

    [JsonProperty("down", NullValueHandling = NullValueHandling.Ignore)]
    public string Down { get; set; }

    [JsonProperty("north", NullValueHandling = NullValueHandling.Ignore)]
    public string North { get; set; }

    [JsonProperty("east", NullValueHandling = NullValueHandling.Ignore)]
    public string East { get; set; }

    [JsonProperty("south", NullValueHandling = NullValueHandling.Ignore)]
    public string South { get; set; }

    [JsonProperty("west", NullValueHandling = NullValueHandling.Ignore)]
    public string West { get; set; }

    public BedrockBlockDefTextures() { }
    public BedrockBlockDefTextures(string all) : this(all, all, all, all, all, all) { }
    public BedrockBlockDefTextures(string up, string down, string side) : this(up, down, side, side, side, side) { }
    public BedrockBlockDefTextures(string up, string down, string north, string east, string south, string west) : this()
    {
        Up = up;
        Down = down;
        North = north;
        East = east;
        South = south;
        West = west;
    }
}