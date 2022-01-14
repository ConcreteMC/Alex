using Newtonsoft.Json;
using ResourcePackLib.Core;
using ResourcePackLib.Core.Utils;

namespace ResourcePackLib.Loader.Bedrock.Data;

public class BedrockSoundDefSound : ISoundDefSound
{
    public string Name { get; }
    public bool Stream { get; }
    public bool LoadOnLowMemory { get; }
    public float Volume { get; }
    public int Weight { get; }
}

public class BedrockSoundDef : ISoundDef
{
    public string Category { get; set; }
    
    [JsonProperty("__use_legacy_max_distance")]
    public bool UseLegacyMaxDistance { get; set; }
    
    public List<ISoundDefSound> Sounds { get; set; }
    
    public string Subtitle { get; set; }
}