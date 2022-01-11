using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ResourcePackLib.Loader.Bedrock.Data;

public class BedrockBlockDef
{
    [JsonProperty("sound", NullValueHandling = NullValueHandling.Ignore)]
    public string Sound { get; set; }

    [JsonProperty("carried_textures", NullValueHandling = NullValueHandling.Ignore)]
    public BedrockBlockDefTextures CarriedTextures { get; set; }

    [JsonProperty("textures", NullValueHandling = NullValueHandling.Ignore)]
    public BedrockBlockDefTextures Textures { get; set; }

    [JsonProperty("isotropic", NullValueHandling = NullValueHandling.Ignore)]
    public BedrockBlockDefIsotropic Isotropic { get; set; }

    [JsonProperty("brightness_gamma", NullValueHandling = NullValueHandling.Ignore)]
    public double? BrightnessGamma { get; set; }
}