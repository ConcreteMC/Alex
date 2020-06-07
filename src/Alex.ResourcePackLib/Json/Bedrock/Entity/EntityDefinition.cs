using System.Collections.Generic;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
    public class EntityDescriptionWrapper
    {
        [JsonProperty("description")]
        public EntityDescription Description { get; set; }
    }

    public class EntityDescription
    {
        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("min_engine_version")]
        public string MinEngineVersion { get; set; }

        [JsonProperty("materials")]
        public Dictionary<string, string> Materials { get; set; }

        [JsonProperty("textures")]
        public Dictionary<string, string> Textures { get; set; }

        [JsonProperty("geometry")]
        public Dictionary<string, string> Geometry { get; set; }

       // [JsonProperty("render_controllers")]
       // public string[] RenderControllers { get; set; }
    }
}