namespace Alex
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using J = Newtonsoft.Json.JsonPropertyAttribute;
    using R = Newtonsoft.Json.Required;
    using N = Newtonsoft.Json.NullValueHandling;

    public partial class AssetIndex
    {
        [J("objects")] public Dictionary<string, AssetIndexObject> Objects { get; set; }
    }

    public partial class AssetIndexObject
    {
        [J("hash")] public string Hash { get; set; }
        [J("size")] public long Size { get; set; }  
    }

    public partial class AssetIndex
    {
        public static AssetIndex FromJson(string json) => JsonConvert.DeserializeObject<AssetIndex>(json, AssetIndexConverter.Settings);
    }

    public static class AssetIndexSerialize
    {
        public static string ToJson(this AssetIndex self) => JsonConvert.SerializeObject(self, AssetIndexConverter.Settings);
    }

    internal static class AssetIndexConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}