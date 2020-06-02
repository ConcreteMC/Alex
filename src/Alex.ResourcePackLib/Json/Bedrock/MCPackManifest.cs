namespace Alex.ResourcePackLib.Json.Bedrock
{
    using System;
    using Newtonsoft.Json;

    public class McPackManifest
    {
        [JsonProperty("header")]
        public Header Header { get; set; }

        [JsonProperty("modules")]
        public Module[] Modules { get; set; }

        [JsonProperty("format_version")]
        public long FormatVersion { get; set; }
    }

    public class Header
    {
        [JsonProperty("version")]
        public long[] Version { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uuid")]
        public Guid Uuid { get; set; }
    }

    public class Module
    {
        [JsonProperty("version")]
        public long[] Version { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("uuid")]
        public Guid Uuid { get; set; }
    }
}
